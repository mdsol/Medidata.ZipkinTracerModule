using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using Medidata.ZipkinTracerModule.Collector;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class ZipkinClientTests
    {
        private IFixture fixture;
        private ISpanCollectorBuilder spanCollectorBuilder;
        private SpanCollector spanCollectorStub;
        private SpanTracer spanTracerStub;
        private ServiceEndpoint zipkinEndpoint;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanCollectorBuilder = MockRepository.GenerateStub<ISpanCollectorBuilder>();
            zipkinEndpoint = MockRepository.GenerateStub<ServiceEndpoint>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinServer()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(null);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinPort()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(null);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanProcessorBatchSize()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return(null);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullServiceName()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(null);
            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CTOR_WithNonIntegerZipkinPort()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return(fixture.Create<string>());

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CTOR_WithNonIntegerSpanProcessorBatchSize()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return(fixture.Create<string>());

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        public void Init_StartCollector()
        {
            var zipkinClient = SetupZipkinClient();

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
            var spanId = fixture.Create<string>();

            var zipkinClient = SetupZipkinClient();
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId)).Return(expectedSpan);

            var resultSpan = zipkinClient.StartServerSpan(requestName, traceId, parentSpanId, spanId);

            Assert.AreEqual(expectedSpan, resultSpan);
        }

        [TestMethod]
        public void EndClientSpan()
        {
            var zipkinClient = SetupZipkinClient();
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;

            var expectedSpan = new Span();
            var expectedDuration = fixture.Create<int>();

            zipkinClient.EndServerSpan(expectedSpan, expectedDuration);

            spanTracerStub.AssertWasCalled(x => x.SendServerSpan(expectedSpan, expectedDuration));
        }

        private ZipkinClient SetupZipkinClient()
        {
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            return new ZipkinClient(CreateZipkinConfigWithValues(), spanCollectorBuilder);
        }

        private IZipkinConfig CreateZipkinConfigWithValues()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return("123");
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());
            return zipkinConfigStub;
        }
    }
}
