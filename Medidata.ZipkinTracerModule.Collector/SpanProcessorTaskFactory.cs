using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule.Collector
{
    public class SpanProcessorTaskFactory
    {
        internal Task spanProcessorTaskInstance = null;
        internal CancellationTokenSource cancellationTokenSource;

        [ExcludeFromCodeCoverage]  //excluded from code coverage since this class is a 1 liner that starts up a background thread
        public virtual void CreateAndStart(Action action)
        {
            if (spanProcessorTaskInstance == null
                || spanProcessorTaskInstance.Status == TaskStatus.Faulted)
            {
                cancellationTokenSource = new CancellationTokenSource();
                spanProcessorTaskInstance = new Task( () => ActionWrapper(action), cancellationTokenSource.Token, TaskCreationOptions.LongRunning);
                spanProcessorTaskInstance.Start();
            }
        }

        public virtual void StopTask()
        {
            if ( cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
            }
        }

        internal async void ActionWrapper(Action action)
        {
            while(!cancellationTokenSource.Token.IsCancellationRequested )
            {
                action();
                await Task.Delay(500, cancellationTokenSource.Token);
            } 
        }

        public virtual bool IsTaskCancelled()
        {
            return cancellationTokenSource.IsCancellationRequested;
        }
    }
}
