using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule.Collector
{
    [ExcludeFromCodeCoverage]  //excluded from code coverage since this class is a 1 liner to new up SpanCollector
    public class SpanCollectorBuilder : ISpanCollectorBuilder
    {
        public SpanCollector Build(string zipkinServer, int zipkinPort, int maxProcessorBatchSize)
        {
            return new SpanCollector(ClientProvider.GetInstance(zipkinServer, zipkinPort), maxProcessorBatchSize);
        }
    }
}
