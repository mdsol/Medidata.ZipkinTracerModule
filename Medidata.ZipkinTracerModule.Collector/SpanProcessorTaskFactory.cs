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
        [ExcludeFromCodeCoverage]  //excluded from code coverage since this class is a 1 liner that starts up a background thread
        public virtual void CreateAndStart(Action action, CancellationTokenSource tokenSource)
        {
            new Task(action, tokenSource.Token, TaskCreationOptions.LongRunning).Start();
        }
    }
}
