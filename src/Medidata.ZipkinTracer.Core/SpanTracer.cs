using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Medidata.ZipkinTracer.Core
{
    public class SpanTracer
    {
        private static readonly Dictionary<Type, AnnotationType> annotationTypes = new Dictionary<Type, AnnotationType>()
        {
            { typeof(bool), AnnotationType.BOOL },
            { typeof(byte[]), AnnotationType.BYTES },
            { typeof(short), AnnotationType.I16 },
            { typeof(int), AnnotationType.I32 },
            { typeof(long), AnnotationType.I64 },
            { typeof(double), AnnotationType.DOUBLE },
            { typeof(string), AnnotationType.STRING }
        };

        private Collector.SpanCollector spanCollector;
        private string serviceName;
        private ServiceEndpoint zipkinEndpoint;
        private IEnumerable<string> zipkinNotToBeDisplayedDomainList;

        public SpanTracer(Collector.SpanCollector spanCollector, ServiceEndpoint zipkinEndpoint, IEnumerable<string> zipkinNotToBeDisplayedDomainList, string domain, string serviceName)
        {
            if ( spanCollector == null) 
            {
                throw new ArgumentNullException("spanCollector is null");
            }
            this.spanCollector = spanCollector;

            if ( String.IsNullOrEmpty(serviceName)) 
            {
                throw new ArgumentNullException("serviceName is null or empty");
            }
            this.zipkinEndpoint = zipkinEndpoint;

            if ( zipkinEndpoint == null) 
            {
                throw new ArgumentNullException("zipkinEndpoint is null");
            }
            this.zipkinNotToBeDisplayedDomainList = zipkinNotToBeDisplayedDomainList;
            var cleanDomain = CleanServiceName(domain);
            this.serviceName = string.IsNullOrWhiteSpace(cleanDomain) ? serviceName : cleanDomain;
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
            var clientServiceName = CleanServiceName(remoteUri.Host);

            var annotation = new Annotation
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

        private string CleanServiceName(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return null;
            }

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

        public virtual void Record(Span span, string value)
        {
            if (span == null)
                throw new ArgumentNullException("span", "In order to record an annotation, the span must be not null.");

            span.Annotations.Add(new Annotation()
            {
                Host = zipkinEndpoint.GetLocalEndpoint(serviceName),
                Timestamp = GetTimeStamp(),
                Value = value
            });
        }

        public void RecordBinary<T>(Span span, string key, T value)
        {
             if (span == null)
                throw new ArgumentNullException("span", "In order to record a binary annotation, the span must be not null.");

            span.Binary_annotations.Add(new BinaryAnnotation()
            {
                Host = zipkinEndpoint.GetLocalEndpoint(serviceName),
                Annotation_type = GetAnnotationType(typeof(T)),
                Key = key,
                Value = value?.ToString()
            });
        }

        private AnnotationType GetAnnotationType(Type type)
        {
            return annotationTypes.ContainsKey(type) ? annotationTypes[type] : AnnotationType.STRING;
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
