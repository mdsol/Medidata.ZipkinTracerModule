using System;
using System.Collections.Generic;
using System.Linq;
using Medidata.ZipkinTracer.Models;

namespace Medidata.ZipkinTracer.Core
{
    public class SpanTracer
    {
        private SpanCollector spanCollector;
        private string serviceName;
        private ushort servicePort;
        private ServiceEndpoint zipkinEndpoint;
        private IEnumerable<string> zipkinNotToBeDisplayedDomainList;

        public SpanTracer(SpanCollector spanCollector, ServiceEndpoint zipkinEndpoint, IEnumerable<string> zipkinNotToBeDisplayedDomainList, Uri domain)
        {
            if (spanCollector == null) throw new ArgumentNullException(nameof(spanCollector));
            if (zipkinEndpoint == null) throw new ArgumentNullException(nameof(zipkinEndpoint));
            if (zipkinNotToBeDisplayedDomainList == null) throw new ArgumentNullException(nameof(zipkinNotToBeDisplayedDomainList));
            if (domain == null) throw new ArgumentNullException(nameof(domain));

            this.spanCollector = spanCollector;
            this.zipkinEndpoint = zipkinEndpoint;
            this.zipkinNotToBeDisplayedDomainList = zipkinNotToBeDisplayedDomainList;
            var domainHost = domain.Host;
            this.serviceName = CleanServiceName(domainHost);
            this.servicePort = (ushort)domain.Port;
        }

        public virtual Span ReceiveServerSpan(string spanName, string traceId, string parentSpanId, string spanId, Uri requestUri)
        {
            var newSpan = CreateNewSpan(spanName, traceId, parentSpanId, spanId);
            var serviceEndpoint = zipkinEndpoint.GetLocalEndpoint(serviceName, (ushort)requestUri.Port);

            var annotation = new Annotation()
            {
                Host = serviceEndpoint,
                Value = ZipkinConstants.ServerReceive
            };

            newSpan.Annotations.Add(annotation);

            AddBinaryAnnotation("http.path", requestUri.AbsolutePath, newSpan, serviceEndpoint);

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
                Value = ZipkinConstants.ServerSend
            };

            span.Annotations.Add(annotation);

            spanCollector.Collect(span);
        }

        public virtual Span SendClientSpan(string spanName, string traceId, string parentSpanId, string spanId, Uri remoteUri)
        {
            var newSpan = CreateNewSpan(spanName, traceId, parentSpanId, spanId);
            var serviceEndpoint = zipkinEndpoint.GetLocalEndpoint(serviceName, (ushort)remoteUri.Port);
            var clientServiceName = CleanServiceName(remoteUri.Host);

            var annotation = new Annotation
            {
                Host = serviceEndpoint,
                Value = ZipkinConstants.ClientSend
            };

            newSpan.Annotations.Add(annotation);
            AddBinaryAnnotation("http.path", remoteUri.AbsolutePath, newSpan, serviceEndpoint);
            AddBinaryAnnotation("sa", "1", newSpan, zipkinEndpoint.GetRemoteEndpoint(remoteUri, clientServiceName));

            return newSpan;
        }

        private string CleanServiceName(string host)
        {
            foreach (var domain in zipkinNotToBeDisplayedDomainList)
            {
                if (host.Contains(domain))
                {
                    return host.Replace(domain, string.Empty);
                }
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
                Value = ZipkinConstants.ClientReceive
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
                Host = zipkinEndpoint.GetLocalEndpoint(serviceName, servicePort),
                Value = value
            });
        }

        public void RecordBinary<T>(Span span, string key, T value)
        {
             if (span == null)
                throw new ArgumentNullException("span", "In order to record a binary annotation, the span must be not null.");

            span.Annotations.Add(new BinaryAnnotation()
            {
                Host = zipkinEndpoint.GetLocalEndpoint(serviceName, servicePort),
                Key = key,
                Value = value
            });
        }

        private Span CreateNewSpan(string spanName, string traceId, string parentSpanId, string spanId)
        {
            var newSpan = new Span();
            newSpan.Id = Int64.Parse(spanId, System.Globalization.NumberStyles.HexNumber);
            newSpan.TraceId = Int64.Parse(traceId, System.Globalization.NumberStyles.HexNumber);

            if (!String.IsNullOrEmpty(parentSpanId))
            {
                newSpan.ParentId = Int64.Parse(parentSpanId, System.Globalization.NumberStyles.HexNumber);
            }

            newSpan.Name = spanName;
            return newSpan;
        }

        private void AddBinaryAnnotation<T>(string key, T value, Span span, Endpoint endpoint)
        {
            var binaryAnnotation = new BinaryAnnotation()
            {
                Host = endpoint,
                Key = key,
                Value = value
            };

            span.Annotations.Add(binaryAnnotation);
        }
    }
}
