using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medidata.ZipkinTracerModule
{
    public class SpanTracer
    {
        private Collector.SpanCollector spanCollector;
        private string serviceName;

        public SpanTracer(Collector.SpanCollector spanCollector, string serviceName)
        {
            // TODO: Complete member initialization
            //this.spanCollector = spanCollector;
            //this.serviceName = serviceName;
        }

        public virtual Span StartClientSpan(string requestName, string traceId, string parentSpanId)
        {
            throw new NotImplementedException();
        }

        public virtual void EndClientSpan(Span span, int duration)
        {
            throw new NotImplementedException();
        }
    }
}
