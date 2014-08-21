using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public interface ISpanCollectorBuilder
    {
        SpanCollector Build(string zipkinServer, int zipkinPort, int maxProcessorBatchSize);
    }
}
