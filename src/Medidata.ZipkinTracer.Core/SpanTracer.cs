using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Medidata.ZipkinTracer.Core
{
    public class SpanTracer
    {
        private Medidata.ZipkinTracer.Core.Collector.SpanCollector spanCollector;
        private string serviceName;
        private ServiceEndpoint zipkinEndpoint;

        public SpanTracer(Medidata.ZipkinTracer.Core.Collector.SpanCollector spanCollector, string serviceName, ServiceEndpoint zipkinEndpoint)
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

        public virtual Span ReceiveServerSpan(string requestName, string traceId, string parentSpanId, string spanId)
        {
            var newSpan = CreateNewSpan(requestName, traceId, parentSpanId, spanId);

            var annotation = new Annotation()
            {
                Host = zipkinEndpoint.GetEndpoint(serviceName),
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.SERVER_RECV
            };
            newSpan.Annotations.Add(annotation);

            AddBinaryAnnotation("trace_id", traceId, newSpan);
            AddBinaryAnnotation("span_id", spanId, newSpan);
            AddBinaryAnnotation("parent_id", parentSpanId, newSpan);

            return newSpan;
        }

        public virtual void SendServerSpan(Span span, int duration)
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

        public virtual Span SendClientSpan(string requestName, string traceId, string parentSpanId, string spanId)
        {
            var newSpan = CreateNewSpan(requestName, traceId, parentSpanId, spanId);

            var annotation = new Annotation()
            {
                Host = zipkinEndpoint.GetEndpoint(serviceName),
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.CLIENT_SEND
            };

            newSpan.Annotations.Add(annotation);

            AddBinaryAnnotation("trace_id", traceId, newSpan);
            AddBinaryAnnotation("span_id", spanId, newSpan);
            AddBinaryAnnotation("parent_id", parentSpanId, newSpan);

            return newSpan;
        }

        public virtual void ReceiveClientSpan(Span span, int duration)
        {
            var annotation = new Annotation()
            {
                Host = zipkinEndpoint.GetEndpoint(serviceName),
                Duration = duration,  //duration is currently not supported by zipkin UI
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.CLIENT_RECV
            };

            span.Annotations.Add(annotation);

            spanCollector.Collect(span);
        }

        private Span CreateNewSpan(string requestName, string traceId, string parentSpanId, string spanId)
        {
            var newSpan = new Span();
            newSpan.Id = Int64.Parse(spanId, System.Globalization.NumberStyles.HexNumber);
            newSpan.Trace_id = Int64.Parse(traceId, System.Globalization.NumberStyles.HexNumber);

            if (!String.IsNullOrEmpty(parentSpanId))
            {
                newSpan.Parent_id = Int64.Parse(parentSpanId, System.Globalization.NumberStyles.HexNumber);
            }

            newSpan.Name = requestName;
            newSpan.Annotations = new List<Annotation>();
            newSpan.Binary_annotations = new List<BinaryAnnotation>();
            return newSpan;
        }

        private long GetTimeStamp()
        {
            var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            //return Convert.ToInt64(t.TotalMilliseconds * 1000);

            //the version in sandbox expects milliseconds.  The new versions of zipkin expects microseconds.
            return Convert.ToInt64(t.TotalMilliseconds);
        }

        internal static byte[] ConvertToBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private void AddBinaryAnnotation(string key, string value, Span span)
        {
            var binaryAnnotation = new BinaryAnnotation()
            {
                Host = zipkinEndpoint.GetEndpoint(serviceName),
                Annotation_type = AnnotationType.STRING,
                Key = key,
                Value = ConvertToBytes(value)
            };

            span.Binary_annotations.Add(binaryAnnotation);
        }
    }
}
