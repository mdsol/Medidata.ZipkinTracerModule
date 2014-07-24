using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Medidata.ZipkinTracerModule.Collector;
using Rhino.Mocks;

namespace Medidata.ZipkinTracerModule.Test
{
    [TestClass]
    public class SpanTracerTests
    {
        private IFixture fixture;
        private SpanCollector spanCollectorStub;
        private ZipkinEndpoint zipkinEndpointStub;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>());
            zipkinEndpointStub = MockRepository.GenerateStub<ZipkinEndpoint>();
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
        public void StartClientSpan()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();

            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            zipkinEndpointStub.Expect(x => x.GetEndpoint(serviceName)).Return(new Endpoint() { Service_name = serviceName });

            var resultSpan = spanTracer.StartClientSpan(requestName, traceId, parentSpanId);

            Assert.AreEqual(requestName, resultSpan.Name);
            Assert.AreEqual(traceId, resultSpan.Trace_id.ToString());
            Assert.AreEqual(parentSpanId, resultSpan.Parent_id.ToString());

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

        [TestMethod]
        public void EndClientSpan()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, serviceName, zipkinEndpointStub);

            var expectedSpan = new Span() { Annotations = new System.Collections.Generic.List<Annotation>() };
            var expectedDuration = fixture.Create<int>();

            zipkinEndpointStub.Expect(x => x.GetEndpoint(serviceName)).Return(new Endpoint() { Service_name = serviceName });

            spanTracer.EndClientSpan(expectedSpan, expectedDuration);

            spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Matches(y =>
                    ValidateSpan(y, serviceName, expectedDuration)
                    ))
                );
        }

        private bool ValidateSpan(Span y, string serviceName, int duration)
        {
            var annotation = (Annotation)y.Annotations[0];
            var endpoint = (Endpoint)annotation.Host;

            Assert.AreEqual(serviceName, endpoint.Service_name);
            Assert.AreEqual(duration, annotation.Duration);
            Assert.AreEqual(zipkinCoreConstants.CLIENT_RECV, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);

            return true;
        }
    }
}
