using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using Medidata.ZipkinTracer.Core.Collector;
using Medidata.CrossApplicationTracer;

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
        private ITraceProvider traceProvider;
        private string requestName;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanCollectorBuilder = MockRepository.GenerateStub<ISpanCollectorBuilder>();
            zipkinEndpoint = MockRepository.GenerateStub<ServiceEndpoint>();
            traceProvider = MockRepository.GenerateStub<ITraceProvider>();
            requestName = fixture.Create<string>();
        }

        [TestMethod]
        public void CTOR_WithNullZipkinServer()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(null, "123", "123", fixture.Create<string>(), "goo,bar", "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullZipkinPort()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), null, "123", fixture.Create<string>(), "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullSpanProcessorBatchSize()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", null, fixture.Create<string>(), "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullServiceName()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", null, "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullWhiteListCsv()
        {
            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(true);

            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", fixture.Create<string>(), null , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsTrue(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullZipkinSampleRate()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", fixture.Create<string>(), "asfdsa" , null);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNonIntegerZipkinPort()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "asdf", "123", fixture.Create<string>(), "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNonIntegerSpanProcessorBatchSize()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "sfa", fixture.Create<string>(), "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullTraceProvider()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(null, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithTraceIdNullOrEmpty()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            traceProvider.Expect(x => x.TraceId).Return(string.Empty);
            traceProvider.Expect(x => x.IsSampled).Return(true);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithIsSampledFalse()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(false);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, requestName, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void Init_StartCollector()
        {
            var zipkinClient = SetupZipkinClient();

            spanCollectorStub.AssertWasCalled(x => x.Start()); 
        }

        [TestMethod]
        public void Shutdown_StopCollector()
        {
            var zipkinClient = (ZipkinClient) SetupZipkinClient();

            zipkinClient.ShutDown();

            spanCollectorStub.AssertWasCalled(x => x.Stop()); 
        }

        [TestMethod]
        public void StartServerSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.ReceiveServerSpan(Arg<string>.Is.Equal(requestName), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(expectedSpan);

            tracerClient.StartServerTrace();

            Assert.AreEqual(expectedSpan, zipkinClient.serverSpan);
        }

        [TestMethod]
        public void EndServerSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            zipkinClient.serverSpan = new Span();

            var expectedDuration = fixture.Create<int>();

            tracerClient.EndServerTrace(expectedDuration);

            spanTracerStub.AssertWasCalled(x => x.SendServerSpan(zipkinClient.serverSpan, expectedDuration));
        }

        [TestMethod]
        public void StartClientSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.SendClientSpan(Arg<string>.Is.Equal(requestName), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(expectedSpan);

            tracerClient.StartClientTrace();

            Assert.AreEqual(expectedSpan, zipkinClient.clientSpan);
        }

        [TestMethod]
        public void EndClientSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            zipkinClient.clientSpan = new Span();

            var expectedDuration = fixture.Create<int>();

            tracerClient.EndClientTrace(expectedDuration);

            spanTracerStub.AssertWasCalled(x => x.ReceiveClientSpan(zipkinClient.clientSpan, expectedDuration));
        }

        private ITracerClient SetupZipkinClient()
        {
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(true);

            return new ZipkinClient(traceProvider, requestName, CreateZipkinConfigWithDefaultValues(), spanCollectorBuilder);
        }

        private IZipkinConfig CreateZipkinConfigWithDefaultValues()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return("123");
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.DontSampleListCsv).Return("foo,bar,baz");
            zipkinConfigStub.Expect(x => x.ZipkinSampleRate).Return("0.5");
            return zipkinConfigStub;
        }

        private IZipkinConfig CreateZipkinConfigWithValues(string zipkinServerName, string zipkinServerPort, string spanProcessorBatchSize, string serviceName, string filterListCsv, string zipkinSampleRate)
        { 
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(zipkinServerName);
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(zipkinServerPort);
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return(spanProcessorBatchSize);
            zipkinConfigStub.Expect(x => x.ServiceName).Return(serviceName);
            zipkinConfigStub.Expect(x => x.DontSampleListCsv).Return(filterListCsv);
            zipkinConfigStub.Expect(x => x.ZipkinSampleRate).Return(zipkinSampleRate);
            return zipkinConfigStub;
        }
    }
}
