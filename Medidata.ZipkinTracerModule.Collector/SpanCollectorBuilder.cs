using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule.Collector
{
    public class SpanCollectorBuilder : ISpanCollectorBuilder
    {
        public SpanCollector Build(string zipkinServer, int zipkinPort)
        {
            return new SpanCollector(new ClientProvider(zipkinServer, zipkinPort));
        }
    }
}
