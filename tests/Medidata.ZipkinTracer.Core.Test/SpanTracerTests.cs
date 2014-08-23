using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Medidata.ZipkinTracer.Core.Collector;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class SpanTracerTests
    {
        private IFixture fixture;
        private SpanCollector spanCollectorStub;
        private ServiceEndpoint zipkinEndpointStub;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            zipkinEndpointStub = MockRepository.GenerateStub<ServiceEndpoint>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanCollector()
        {
            var spanTracer = new SpanTracer(null, fixture.Create<string>(), zipkinEndpointStub);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullOrEmptyString()
        {
            var spanTracer = new SpanTracer(spanCollectorStub, null, zipkinEndpointStub);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinEndpoint()
        {
            var spanTracer = new SpanTracer(spanCollectorStub, fixture.Create<string>(), null);
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
        }

        [TestMethod]
        public void SendServerSpan()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            var expectedSpan = new Span() { Annotations = new System.Collections.Generic.List<Annotation>() };
            var expectedDuration = fixture.Create<int>();

            zipkinEndpointStub.Expect(x => x.GetEndpoint(serviceName)).Return(new Endpoint() { Service_name = serviceName });

            spanTracer.SendServerSpan(expectedSpan, expectedDuration);

            spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Matches(y =>
                    ValidateSpan(y, serviceName, expectedDuration)
                    ))
                );
        }

        [TestMethod]
        public void SendClientSpan()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();

            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            zipkinEndpointStub.Expect(x => x.GetEndpoint(serviceName)).Return(new Endpoint() { Service_name = serviceName });

            var resultSpan = spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId);
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

            var endpoint = annotation.Host as Endpoint;
            Assert.IsNotNull(endpoint);
            Assert.AreEqual(serviceName, endpoint.Service_name);
        }

        private bool ValidateSpan(Span y, string serviceName, int duration)
        {
            var annotation = (Annotation)y.Annotations[0];
            var endpoint = (Endpoint)annotation.Host;

            Assert.AreEqual(serviceName, endpoint.Service_name);
            Assert.AreEqual(duration, annotation.Duration);
            Assert.AreEqual(zipkinCoreConstants.SERVER_SEND, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);

            return true;
        }
    }
}
