using Medidata.ZipkinTracerModule.Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule
{
    public interface ISpanTracerBuilder
    {
        SpanTracer Build(SpanCollector spanCollector, string serviceName);
    }
}
