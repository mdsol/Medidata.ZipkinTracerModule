using log4net;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core
{
    public class SpanProcessorTaskFactory
    {
        private Task spanProcessorTaskInstance;
        private CancellationTokenSource cancellationTokenSource;
        private ILog logger;
        private const int defaultDelayTime = 500;
        private const int encounteredAnErrorDelayTime = 30000;

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
                int delayTime = defaultDelayTime;
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    logger.Error("Error in SpanProcessorTask", ex);
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
