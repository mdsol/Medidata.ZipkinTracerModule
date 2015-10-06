using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Medidata.ZipkinTracer.Core.Collector;
using Rhino.Mocks;
using System.Linq;
using System.Text;
using log4net;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class SpanTracerTests
    {
        private IFixture fixture;
        private SpanCollector spanCollectorStub;
        private ServiceEndpoint zipkinEndpointStub;
        private ILog logger;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            logger = MockRepository.GenerateStub<ILog>();
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0, logger);
            zipkinEndpointStub = MockRepository.GenerateStub<ServiceEndpoint>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanCollector()
        {
            new SpanTracer(null, fixture.Create<string>(), zipkinEndpointStub);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullOrEmptyString()
        {
            new SpanTracer(spanCollectorStub, null, zipkinEndpointStub);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinEndpoint()
        {
            new SpanTracer(spanCollectorStub, fixture.Create<string>(), null);
        }

        [TestMethod]
        public void ReceiveServerSpan()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();

            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            zipkinEndpointStub.Expect(x => x.GetEndpoint(serviceName)).Return(new Endpoint() { Service_name = serviceName });

            var resultSpan = spanTracer.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId);

            Assert.AreEqual(requestName, resultSpan.Name);
            Assert.AreEqual(Int64.Parse(traceId, System.Globalization.NumberStyles.HexNumber), resultSpan.Trace_id);
            Assert.AreEqual(Int64.Parse(parentSpanId, System.Globalization.NumberStyles.HexNumber), resultSpan.Parent_id);
            Assert.AreEqual(Int64.Parse(spanId, System.Globalization.NumberStyles.HexNumber), resultSpan.Id);

            Assert.AreEqual(1, resultSpan.Annotations.Count);

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.IsNotNull(annotation);
            Assert.AreEqual(zipkinCoreConstants.SERVER_RECV, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);
            Assert.IsNotNull(annotation.Host);

            var endpoint = annotation.Host as Endpoint;
            Assert.IsNotNull(endpoint);
            Assert.AreEqual(serviceName, endpoint.Service_name);

            AssertBinaryAnnotations(resultSpan.Binary_annotations, traceId, spanId, parentSpanId);
        }

        [TestMethod]
        public void SendServerSpan()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            var endpoint = new Endpoint() { Service_name = serviceName };
            var expectedSpan = new Span() { Annotations = new System.Collections.Generic.List<Annotation>() };
            expectedSpan.Annotations.Add(new Annotation() { Host = endpoint, Value = zipkinCoreConstants.SERVER_RECV, Timestamp = 1 });

            zipkinEndpointStub.Expect(x => x.GetEndpoint(serviceName)).Return(new Endpoint() { Service_name = serviceName });

            spanTracer.SendServerSpan(expectedSpan);

            spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Matches(y =>
                    ValidateSendServerSpan(y, serviceName)
                    ))
                );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendServerSpan_NullSpan()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            spanTracer.SendServerSpan(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendServerSpan_NullAnnotation()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            var expectedSpan = new Span();

            spanTracer.SendServerSpan(expectedSpan);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendServerSpan_InvalidAnnotation()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            var expectedSpan = new Span() { Annotations = new System.Collections.Generic.List<Annotation>() };

            spanTracer.SendServerSpan(expectedSpan);
        }

        [TestMethod]
        public void SendClientSpan()
        {
            var serviceName = fixture.Create<string>();
            var clientServiceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();

            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            zipkinEndpointStub.Expect(x => x.GetEndpoint(clientServiceName)).Return(new Endpoint() { Service_name = clientServiceName });

            var resultSpan = spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId, clientServiceName);
            Assert.AreEqual(requestName, resultSpan.Name);
            Assert.AreEqual(Int64.Parse(traceId, System.Globalization.NumberStyles.HexNumber), resultSpan.Trace_id);
            Assert.AreEqual(Int64.Parse(parentSpanId, System.Globalization.NumberStyles.HexNumber), resultSpan.Parent_id);
            Assert.AreEqual(Int64.Parse(spanId, System.Globalization.NumberStyles.HexNumber), resultSpan.Id);

            Assert.AreEqual(1, resultSpan.Annotations.Count);

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.IsNotNull(annotation);
            Assert.AreEqual(zipkinCoreConstants.CLIENT_SEND, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);
            Assert.IsNotNull(annotation.Host);
            Assert.AreEqual(0, resultSpan.Binary_annotations.Count);

            var endpoint = annotation.Host as Endpoint;
            Assert.IsNotNull(endpoint);
            Assert.AreEqual(clientServiceName, endpoint.Service_name);
        }

        [TestMethod]
        public void ReceiveClientSpan()
        {
            var serviceName = fixture.Create<string>();
            var clientServiceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);
            var endpoint = new Endpoint() { Service_name = clientServiceName };
            var expectedSpan = new Span()
            {
                Annotations = new System.Collections.Generic.List<Annotation>()
            };
            expectedSpan.Annotations.Add(new Annotation() { Host = endpoint, Value = zipkinCoreConstants.CLIENT_SEND, Timestamp = 1 });

            zipkinEndpointStub.Expect(x => x.GetEndpoint(clientServiceName)).Return(endpoint);

            spanTracer.ReceiveClientSpan(expectedSpan);

            spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Matches(y =>
                    ValidateReceiveClientSpan(y, clientServiceName)
                    ))
                );
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceiveClientSpan_NullAnnotation()
        {
            var serviceName = fixture.Create<string>();
            var clientServiceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);
            var endpoint = new Endpoint() { Service_name = clientServiceName };
            var expectedSpan = new Span()
            {
                Annotations = null
            };

            zipkinEndpointStub.Expect(x => x.GetEndpoint(clientServiceName)).Return(endpoint);

            spanTracer.ReceiveClientSpan(expectedSpan);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceiveClientSpan_EmptyAnnotationsList()
        {
            var serviceName = fixture.Create<string>();
            var clientServiceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);
            var endpoint = new Endpoint() { Service_name = clientServiceName };
            var expectedSpan = new Span()
            {
                Annotations = new System.Collections.Generic.List<Annotation>()
            };

            zipkinEndpointStub.Expect(x => x.GetEndpoint(clientServiceName)).Return(endpoint);

            spanTracer.ReceiveClientSpan(expectedSpan);
        }

        private bool ValidateReceiveClientSpan(Span y, string serviceName)
        {
            var firstannotation = (Annotation)y.Annotations[0];
            var firstEndpoint = (Endpoint)firstannotation.Host;

            Assert.AreEqual(serviceName, firstEndpoint.Service_name);
            Assert.AreEqual(zipkinCoreConstants.CLIENT_SEND, firstannotation.Value);
            Assert.IsNotNull(firstannotation.Timestamp);

            var secondAnnotation = (Annotation)y.Annotations[1];
            var secondEndpoint = (Endpoint)secondAnnotation.Host;

            Assert.AreEqual(serviceName, secondEndpoint.Service_name);
            Assert.AreEqual(zipkinCoreConstants.CLIENT_RECV, secondAnnotation.Value);
            Assert.IsNotNull(secondAnnotation.Timestamp);

            return true;
        }

        private bool ValidateSendServerSpan(Span y, string serviceName)
        {
            var firstAnnotation = (Annotation)y.Annotations[0];
            var firstEndpoint = (Endpoint)firstAnnotation.Host;

            Assert.AreEqual(serviceName, firstEndpoint.Service_name);
            Assert.AreEqual(zipkinCoreConstants.SERVER_RECV, firstAnnotation.Value);
            Assert.IsNotNull(firstAnnotation.Timestamp);

            var secondAnnotation = (Annotation)y.Annotations[1];
            var secondEndpoint = (Endpoint)secondAnnotation.Host;

            Assert.AreEqual(serviceName, secondEndpoint.Service_name);
            Assert.AreEqual(zipkinCoreConstants.SERVER_SEND, secondAnnotation.Value);
            Assert.IsNotNull(secondAnnotation.Timestamp);

            return true;
        }

        private void AssertBinaryAnnotations(System.Collections.Generic.List<BinaryAnnotation> list, string traceId, string spanId, string parentSpanId)
        {
            Assert.AreEqual(3, list.Count);

            Assert.AreEqual(traceId, list.Where(x => x.Key.Equals("trace_id")).Select(x => Encoding.Default.GetString(x.Value)).First());
            Assert.AreEqual(spanId, list.Where(x => x.Key.Equals("span_id")).Select(x => Encoding.Default.GetString(x.Value)).First());
            Assert.AreEqual(parentSpanId, list.Where(x => x.Key.Equals("parent_id")).Select(x => Encoding.Default.GetString(x.Value)).First());
        }
    }
}
