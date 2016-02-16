using System;
using log4net;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public interface ISpanCollectorBuilder
    {
        SpanCollector Build(Uri uri, uint maxProcessorBatchSize, ILog logger);
    }
}
