using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Medidata.ZipkinTracer.Core.Logging;
using Medidata.ZipkinTracer.Core.Helpers;

namespace Medidata.ZipkinTracer.Core
{
    public class SpanProcessorTaskFactory
    {
        private Task spanProcessorTaskInstance;
        private CancellationTokenSource cancellationTokenSource;
        private ILog logger;
        private const int defaultDelayTime = 500;
        private const int encounteredAnErrorDelayTime = 30000;

        readonly object sync = new object();

        public SpanProcessorTaskFactory(ILog logger, CancellationTokenSource cancellationTokenSource)
        {
            this.logger = logger ?? LogProvider.GetCurrentClassLogger();
            this.cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
        }

        public SpanProcessorTaskFactory()
            :this(LogProvider.GetCurrentClassLogger(), new CancellationTokenSource())
        {
        }

        [ExcludeFromCodeCoverage]  //excluded from code coverage since this class is a 1 liner that starts up a background thread
        public virtual void CreateAndStart(Action action)
        {
            SyncHelper.ExecuteSafely(sync, () => spanProcessorTaskInstance == null || spanProcessorTaskInstance.Status == TaskStatus.Faulted,
                () =>
                {
                    spanProcessorTaskInstance = Task.Factory.StartNew(() => ActionWrapper(action), cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                });
        }

        public virtual void StopTask()
        {
            SyncHelper.ExecuteSafely(sync, () => cancellationTokenSource.Token.CanBeCanceled, () => cancellationTokenSource.Cancel());
        }

        internal async void ActionWrapper(Action action)
        {
            while (!IsTaskCancelled())
            {
                int delayTime = defaultDelayTime;
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    logger.ErrorException("Error in SpanProcessorTask", ex);
                    delayTime = encounteredAnErrorDelayTime;
                }

                // stop loop if task is cancelled while delay is in process
                try
                {
                    await Task.Delay(delayTime, cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                
            }
        }

        public virtual bool IsTaskCancelled()
        {
            return cancellationTokenSource.IsCancellationRequested;
        }
    }
}
