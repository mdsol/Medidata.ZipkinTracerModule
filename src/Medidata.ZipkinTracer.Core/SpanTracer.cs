using System;
using System.Collections.Generic;
using System.Linq;

namespace Medidata.ZipkinTracer.Core
{
    public class SpanTracer
    {
        private Collector.SpanCollector spanCollector;
        private string serviceName;
        private ServiceEndpoint zipkinEndpoint;
        private IEnumerable<string> zipkinNotToBeDisplayedDomainList;

        public SpanTracer(Collector.SpanCollector spanCollector, string serviceName, ServiceEndpoint zipkinEndpoint, IEnumerable<string> zipkinNotToBeDisplayedDomainList)
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
            this.zipkinNotToBeDisplayedDomainList = zipkinNotToBeDisplayedDomainList;
        }

        public virtual Span ReceiveServerSpan(string spanName, string traceId, string parentSpanId, string spanId, Uri requestUri)
        {
            var newSpan = CreateNewSpan(spanName, traceId, parentSpanId, spanId);
            var serviceEndpoint = zipkinEndpoint.GetLocalEndpoint(serviceName, (short)requestUri.Port);

            var annotation = new Annotation()
            {
                Host = serviceEndpoint,
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.SERVER_RECV
            };
            newSpan.Annotations.Add(annotation);

            AddBinaryAnnotation("http.uri", requestUri.AbsolutePath, newSpan, serviceEndpoint);

            return newSpan;
        }

        public virtual void SendServerSpan(Span span)
        {
            if (span == null)
            {
                throw new ArgumentNullException("Null server span");
            }

            if (span.Annotations == null || !span.Annotations.Any())
            {
                throw new ArgumentException("Invalid server span: Annotations list is invalid.");
            }

            var annotation = new Annotation()
            {
                Host = span.Annotations.First().Host,
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.SERVER_SEND
            };

            span.Annotations.Add(annotation);

            spanCollector.Collect(span);
        }

        public virtual Span SendClientSpan(string spanName, string traceId, string parentSpanId, string spanId, Uri remoteUri)
        {
            var newSpan = CreateNewSpan(spanName, traceId, parentSpanId, spanId);
            var serviceEndpoint = zipkinEndpoint.GetLocalEndpoint(serviceName);
            var clientServiceName = GetClientServiceName(remoteUri);

            var annotation = new Annotation()
            {
                Host = serviceEndpoint,
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.CLIENT_SEND
            };

            newSpan.Annotations.Add(annotation);
            AddBinaryAnnotation("http.uri", remoteUri.AbsolutePath, newSpan, serviceEndpoint);
            AddBinaryAnnotation("sa", "1", newSpan, zipkinEndpoint.GetRemoteEndpoint(remoteUri, clientServiceName));

            return newSpan;
        }

        private string GetClientServiceName(Uri uri)
        {
            var host = uri.Host;
            foreach (var domain in zipkinNotToBeDisplayedDomainList)
            {
                host = host.Replace(domain, "");
            }
            return host;
        }

        public virtual void ReceiveClientSpan(Span span, int statusCode)
        {
            if (span == null)
            {
                throw new ArgumentNullException("Null client span");
            }

            if (span.Annotations == null || !span.Annotations.Any())
            {
                throw new ArgumentException("Invalid client span: Annotations list is invalid.");
            }

            var annotation = new Annotation()
            {
                Host = span.Annotations.First().Host,
                Timestamp = GetTimeStamp(),
                Value = zipkinCoreConstants.CLIENT_RECV
            };

            span.Annotations.Add(annotation);
            AddBinaryAnnotation("http.status", statusCode.ToString(), span, span.Annotations.First().Host);

            spanCollector.Collect(span);
        }

        private Span CreateNewSpan(string spanName, string traceId, string parentSpanId, string spanId)
        {
            var newSpan = new Span();
            newSpan.Id = Int64.Parse(spanId, System.Globalization.NumberStyles.HexNumber);
            newSpan.Trace_id = Int64.Parse(traceId, System.Globalization.NumberStyles.HexNumber);

            if (!String.IsNullOrEmpty(parentSpanId))
            {
                newSpan.Parent_id = Int64.Parse(parentSpanId, System.Globalization.NumberStyles.HexNumber);
            }

            newSpan.Name = spanName;
            newSpan.Annotations = new List<Annotation>();
            newSpan.Binary_annotations = new List<BinaryAnnotation>();
            return newSpan;
        }

        private long GetTimeStamp()
        {
            var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Convert.ToInt64(t.TotalMilliseconds * 1000);
        }

        private void AddBinaryAnnotation(string key, string value, Span span, Endpoint endpoint)
        {
            var binaryAnnotation = new BinaryAnnotation
            {
                Host = endpoint,
                Annotation_type = AnnotationType.STRING,
                Key = key,
                Value = value
            };

            span.Binary_annotations.Add(binaryAnnotation);
        }
    }
}
