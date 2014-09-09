using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using Medidata.ZipkinTracer.Core.Collector;

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
            var zipkinConfigStub = CreateZipkinConfigWithValues(null, "123", "123", fixture.Create<string>(), "goo,bar" );

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinPort()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), null, "123", fixture.Create<string>(), "goo,bar" );

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanProcessorBatchSize()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", null, fixture.Create<string>(), "goo,bar" );

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullServiceName()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", null, "goo,bar" );

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullWhiteListCsv()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", fixture.Create<string>(), null );

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CTOR_WithNonIntegerZipkinPort()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "asdf", "123", fixture.Create<string>(), "goo,bar" );

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(zipkinConfigStub, spanCollectorBuilder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CTOR_WithNonIntegerSpanProcessorBatchSize()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "sfa", fixture.Create<string>(), "goo,bar" );

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

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
        public void StartServerSpan()
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
        public void EndServerSpan()
        {
            var zipkinClient = SetupZipkinClient();
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;

            var expectedSpan = new Span();
            var expectedDuration = fixture.Create<int>();

            zipkinClient.EndServerSpan(expectedSpan, expectedDuration);

            spanTracerStub.AssertWasCalled(x => x.SendServerSpan(expectedSpan, expectedDuration));
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
            spanTracerStub.Expect(x => x.SendClientSpan(requestName, traceId, parentSpanId, spanId)).Return(expectedSpan);

            var resultSpan = zipkinClient.StartClientSpan(requestName, traceId, parentSpanId, spanId);

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

            zipkinClient.EndClientSpan(expectedSpan, expectedDuration);

            spanTracerStub.AssertWasCalled(x => x.ReceiveClientSpan(expectedSpan, expectedDuration));
        }

        private ZipkinClient SetupZipkinClient()
        {
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            return new ZipkinClient(CreateZipkinConfigWithDefaultValues(), spanCollectorBuilder);
        }

        private IZipkinConfig CreateZipkinConfigWithDefaultValues()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return("123");
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.FilterListCsv).Return("foo,bar,baz");
            return zipkinConfigStub;
        }

        private IZipkinConfig CreateZipkinConfigWithValues(string zipkinServerName, string zipkinServerPort, string spanProcessorBatchSize, string serviceName, string filterListCsv)
        { 
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(zipkinServerName);
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(zipkinServerPort);
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return(spanProcessorBatchSize);
            zipkinConfigStub.Expect(x => x.ServiceName).Return(serviceName);
            zipkinConfigStub.Expect(x => x.FilterListCsv).Return(filterListCsv);
            return zipkinConfigStub;
        }
    }
}
