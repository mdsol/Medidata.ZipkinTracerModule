using System;
using log4net;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public interface ISpanCollectorBuilder
    {
        SpanCollector Build(string zipkinServer, int zipkinPort, int maxProcessorBatchSize, ILog logger, Uri proxyServer = null, string proxyType = null);
    }
}
