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
        private IZipkinEndpoint zipkinEndpoint;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>());
            zipkinEndpoint = MockRepository.GenerateStub<IZipkinEndpoint>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanCollector()
        {
            var spanTracer = new SpanTracer(null, fixture.Create<string>(), zipkinEndpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullOrEmptyString()
        {
            var spanTracer = new SpanTracer(spanCollectorStub, null, zipkinEndpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinEndpoint()
        {
            var spanTracer = new SpanTracer(spanCollectorStub, fixture.Create<string>(), null);
        }
    }
}
