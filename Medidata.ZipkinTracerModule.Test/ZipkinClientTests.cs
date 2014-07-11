using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using Medidata.ZipkinTracerModule.Collector;

namespace Medidata.ZipkinTracerModule.Test
{
    [TestClass]
    public class ZipkinClientTests
    {
        private IFixture fixture;
        private ISpanCollectorBuilder spanCollectorBuilder;
        private SpanCollector spanCollectorStub;
        private ISpanTracerBuilder spanTracerBuilder;
        private SpanTracer spanTracerStub;
        private IZipkinEndpoint zipkinEndpoint;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanCollectorBuilder = MockRepository.GenerateStub<ISpanCollectorBuilder>();
            spanTracerBuilder = MockRepository.GenerateStub<ISpanTracerBuilder>();
            zipkinEndpoint = MockRepository.GenerateStub<IZipkinEndpoint>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinServer()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(null);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder, spanTracerBuilder, zipkinEndpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinPort()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(null);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder, spanTracerBuilder, zipkinEndpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullServiceName()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(null);
            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder, spanTracerBuilder, zipkinEndpoint);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CTOR_WithNonIntegerZipkinPort()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder, spanTracerBuilder, zipkinEndpoint);
        }

        [TestMethod]
        public void Init_StartCollector()
        {
            var zipkinClient = SetupZipkinClient();

            zipkinClient.Init();

            spanCollectorStub.AssertWasCalled(x => x.Start()); 
        }

        [TestMethod]
        public void Init_StopCollector()
        {
            var zipkinClient = SetupZipkinClient();

            zipkinClient.ShutDown();

            spanCollectorStub.AssertWasCalled(x => x.Stop()); 
        }

        [TestMethod]
        public void StartClientSpan()
        {
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();

            var zipkinClient = SetupZipkinClient();

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.StartClientSpan(requestName, traceId, parentSpanId)).Return(expectedSpan);

            var resultSpan = zipkinClient.StartClientSpan(requestName, traceId, parentSpanId);

            Assert.AreEqual(expectedSpan, resultSpan);
        }

        [TestMethod]
        public void EndClientSpan()
        {
            var zipkinClient = SetupZipkinClient();

            var expectedSpan = new Span();
            var expectedDuration = fixture.Create<int>();

            zipkinClient.EndClientSpan(expectedSpan, expectedDuration);

            spanTracerStub.AssertWasCalled(x => x.EndClientSpan(expectedSpan, expectedDuration));
        }

        private ZipkinClient SetupZipkinClient()
        {
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>());
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<IZipkinEndpoint>());
            spanTracerBuilder.Expect(x => x.Build(Arg<SpanCollector>.Is.Anything, Arg<string>.Is.Anything, Arg<IZipkinEndpoint>.Is.Anything)).Return(spanTracerStub);
            return new ZipkinClient(CreateZipkinConfigWithValues(), spanCollectorBuilder, spanTracerBuilder, MockRepository.GenerateStub<IZipkinEndpoint>());
        }

        private IZipkinConfig CreateZipkinConfigWithValues()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());
            return zipkinConfigStub;
        }
    }
}
