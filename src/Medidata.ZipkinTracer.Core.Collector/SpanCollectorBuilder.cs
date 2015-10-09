using System;
using System.Diagnostics.CodeAnalysis;
using log4net;

namespace Medidata.ZipkinTracer.Core.Collector
{
    [ExcludeFromCodeCoverage]  //excluded from code coverage since this class is a 1 liner to new up SpanCollector
    public class SpanCollectorBuilder : ISpanCollectorBuilder
    {
        public SpanCollector Build(string zipkinServer, int zipkinPort, int maxProcessorBatchSize, ILog logger, Uri proxyServer = null)
        {
            return SpanCollector.GetInstance(ClientProvider.GetInstance(zipkinServer, zipkinPort, proxyServer), maxProcessorBatchSize, logger);
        }
    }
}
