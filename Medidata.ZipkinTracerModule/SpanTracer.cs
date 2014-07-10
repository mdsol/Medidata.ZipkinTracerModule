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
            if ( spanCollector == null) 
            {
                throw new ArgumentNullException("spanCollector is null");
            }
            if ( String.IsNullOrEmpty(serviceName)) 
            {
                throw new ArgumentNullException("serviceName is null or empty");
            }
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
