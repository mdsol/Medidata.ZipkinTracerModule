using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule.Collector
{
    public class SpanProcessorTaskFactory
    {
        public virtual void CreateAndStart(Action action, CancellationTokenSource tokenSource)
        {
            new Task(action, tokenSource.Token, TaskCreationOptions.LongRunning).Start();
        }
    }
}
