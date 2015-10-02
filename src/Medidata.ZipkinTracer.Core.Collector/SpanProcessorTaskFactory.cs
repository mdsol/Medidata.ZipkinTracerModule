using log4net;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public class SpanProcessorTaskFactory
    {
        private Task spanProcessorTaskInstance;
        private CancellationTokenSource cancellationTokenSource;
        private ILog logger;

        public SpanProcessorTaskFactory(ILog logger, CancellationTokenSource cancellationTokenSource = null)
        {
            this.logger = logger;

            if (cancellationTokenSource == null)
            {
                this.cancellationTokenSource = new CancellationTokenSource();
            }
            else
            {
                this.cancellationTokenSource = cancellationTokenSource;
            }
        }

        [ExcludeFromCodeCoverage]  //excluded from code coverage since this class is a 1 liner that starts up a background thread
        public virtual void CreateAndStart(Action action)
        {
            if (spanProcessorTaskInstance == null
                || spanProcessorTaskInstance.Status == TaskStatus.Faulted)
            {
                spanProcessorTaskInstance = new Task( () => ActionWrapper(action), cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
                spanProcessorTaskInstance.Start();
            }
        }

        public virtual void StopTask()
        {
            cancellationTokenSource.Cancel();
        }

        internal async void ActionWrapper(Action action)
        {
            while (!IsTaskCancelled())
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in SpanProcessorTask", ex);
                }
                await Task.Delay(500, cancellationTokenSource.Token);
            }
        }

        public virtual bool IsTaskCancelled()
        {
            return cancellationTokenSource.IsCancellationRequested;
        }
    }
}
