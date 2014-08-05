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
        private ServiceEndpoint zipkinEndpoint;

        public SpanTracer(Collector.SpanCollector spanCollector, string serviceName, ServiceEndpoint zipkinEndpoint)
        {
            if ( spanCollector == null) 
            {
                throw new ArgumentNullException("spanCollector is null");
            }

            if ( String.IsNullOrEmpty(serviceName)) 
            {
                throw new ArgumentNullException("serviceName is null or empty");
            }

            if ( zipkinEndpoint == null) 
            {
                throw new ArgumentNullException("zipkinEndpoint is null");
            }

            this.serviceName = serviceName;
            this.spanCollector = spanCollector;
            this.zipkinEndpoint = zipkinEndpoint;
        }

        public virtual Span StartClientSpan(string requestName, string traceId, string parentSpanId, string spanId)
        {
            var newSpan = new Span();
            newSpan.Id = Convert.ToInt64(spanId);
            newSpan.Trace_id = Convert.ToInt64(traceId);

            if ( !String.IsNullOrEmpty(parentSpanId))
            {
                newSpan.Parent_id = Convert.ToInt64(parentSpanId);
            }

            newSpan.Name = requestName;
            newSpan.Annotations = new List<Annotation>();

            var annotation = new Annotation()
            {
                Host = zipkinEndpoint.GetEndpoint(serviceName),
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.SERVER_RECV
            };

            newSpan.Annotations.Add(annotation);

            return newSpan;
        }

        public virtual void EndClientSpan(Span span, int duration)
        {
            var annotation = new Annotation()
            {
                Host = zipkinEndpoint.GetEndpoint(serviceName),
                Duration = duration,  //duration is currently not supported by zipkin UI
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.SERVER_SEND
            };

            span.Annotations.Add(annotation);

            spanCollector.Collect(span);
        }

        private long GetTimeStamp()
        {
            var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToInt64(t.TotalMilliseconds * 1000);
        }

        private long LongRandom(long min, long max)
        {
            byte[] gb = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(gb, 0);
        }
    }
}
