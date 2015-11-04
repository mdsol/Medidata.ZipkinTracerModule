using System;
using log4net;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public interface ISpanCollectorBuilder
    {
        SpanCollector Build(Uri uri, int maxProcessorBatchSize, ILog logger);
    }
}
