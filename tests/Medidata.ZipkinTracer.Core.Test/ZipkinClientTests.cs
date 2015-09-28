using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using Medidata.ZipkinTracer.Core.Collector;
using Medidata.CrossApplicationTracer;
using log4net;

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
        private ILog logger;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanCollectorBuilder = MockRepository.GenerateStub<ISpanCollectorBuilder>();
            zipkinEndpoint = MockRepository.GenerateStub<ServiceEndpoint>();
            traceProvider = MockRepository.GenerateStub<ITraceProvider>();
            logger = MockRepository.GenerateStub<ILog>();
            requestName = fixture.Create<string>();
        }

        [TestMethod]
        public void CTOR_WithNullLogger()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", fixture.Create<string>(), "goo,bar", "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullZipkinServer()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(null, "123", "123", fixture.Create<string>(), "goo,bar", "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullZipkinPort()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), null, "123", fixture.Create<string>(), "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullSpanProcessorBatchSize()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", null, fixture.Create<string>(), "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullServiceName()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", null, "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullWhiteListCsv()
        {
            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(true);

            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", fixture.Create<string>(), null , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsTrue(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullZipkinSampleRate()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "123", fixture.Create<string>(), "asfdsa" , null);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNonIntegerZipkinPort()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "asdf", "123", fixture.Create<string>(), "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNonIntegerSpanProcessorBatchSize()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", "sfa", fixture.Create<string>(), "goo,bar" , "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger,zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullTraceProvider()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(null, requestName, logger,zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithTraceIdNullOrEmpty()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            traceProvider.Expect(x => x.TraceId).Return(string.Empty);
            traceProvider.Expect(x => x.IsSampled).Return(true);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger,zipkinConfigStub, spanCollectorBuilder);
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
            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger,zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_StartCollector()
        {
            var zipkinClient = SetupZipkinClient();

            spanCollectorStub.AssertWasCalled(x => x.Start()); 
        }

        [TestMethod]
        public void CTOR_Collector_Exception()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(true);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);

            var expectedException = new Exception();
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);
            spanCollectorStub.Expect(x => x.Start()).Throw(expectedException); 

            var zipkinClient = new ZipkinClient(traceProvider, requestName, logger,zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void Shutdown_StopCollector()
        {
            var zipkinClient = (ZipkinClient) SetupZipkinClient();

            zipkinClient.ShutDown();

            spanCollectorStub.AssertWasCalled(x => x.Stop()); 
        }

        [TestMethod]
        public void Shutdown_CollectorNullDoesntThrow()
        {
            var zipkinClient = (ZipkinClient) SetupZipkinClient();
            zipkinClient.spanCollector = null;

            zipkinClient.ShutDown();
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
        public void StartServerSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.ReceiveServerSpan(Arg<string>.Is.Equal(requestName), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Throw(new Exception());

            tracerClient.StartServerTrace();

            Assert.AreEqual(null, zipkinClient.serverSpan);
        }

        [TestMethod]
        public void StartServerSpan_IsTraceOnIsFalse()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.isTraceOn = false;

            tracerClient.StartServerTrace();

            Assert.AreEqual(null, zipkinClient.serverSpan);
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
        public void EndServerSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            zipkinClient.clientSpan = new Span();

            var expectedDuration = fixture.Create<int>();

            spanTracerStub.Expect(x => x.SendServerSpan(zipkinClient.serverSpan, expectedDuration)).Throw(new Exception());

            tracerClient.EndServerTrace(expectedDuration);
        }

        [TestMethod]
        public void EndServerSpan_IsTraceOnIsFalse_DoesntThrow()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.isTraceOn = false;

            tracerClient.EndServerTrace(10);
        }

        [TestMethod]
        public void StartClientSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://"+clientServiceName+".xyz.net:8000/");

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.SendClientSpan(Arg<string>.Is.Equal(requestName), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal(clientServiceName))).Return(expectedSpan);

            tracerClient.StartClientTrace(clientServiceUri);

            Assert.AreEqual(expectedSpan, zipkinClient.clientSpan);
        }

        [TestMethod]
        public void StartClientSpan_UsingIpAddress()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "192.168.178.178";
            var clientServiceUri = new Uri("https://" + clientServiceName + ":8000/");

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.SendClientSpan(Arg<string>.Is.Equal(requestName), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal(clientServiceName))).Return(expectedSpan);

            tracerClient.StartClientTrace(clientServiceUri);

            Assert.AreEqual(expectedSpan, zipkinClient.clientSpan);
        }

        [TestMethod]
        public void StartClientSpan_MultipleDomainList()
        {
            var zipkinConfig = CreateZipkinConfigWithDefaultValues();
            zipkinConfig.Expect(x => x.GetInternalDomainList()).Return(new List<string>() { ".abc.net", ".xyz.net" });
            var tracerClient = SetupZipkinClient(zipkinConfig);
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://"+clientServiceName+".xyz.net:8000/");

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.SendClientSpan(Arg<string>.Is.Equal(requestName), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal(clientServiceName))).Return(expectedSpan);

            tracerClient.StartClientTrace(clientServiceUri);

            Assert.AreEqual(expectedSpan, zipkinClient.clientSpan);
        }

        [TestMethod]
        public void StartClientSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://" + clientServiceName + ".xyz.net:8000/");

            spanTracerStub.Expect(x => x.SendClientSpan(Arg<string>.Is.Equal(requestName), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal(clientServiceName))).Throw(new Exception());

            tracerClient.StartClientTrace(clientServiceUri);

            Assert.AreEqual(null, zipkinClient.clientSpan);
        }

        [TestMethod]
        public void StartClientSpan_InvalidClientServiceName()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;

            var expectedSpan = new Span();
            spanTracerStub.Expect(x => x.SendClientSpan(Arg<string>.Is.Equal(requestName), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(expectedSpan);

            tracerClient.StartClientTrace(null);

            Assert.AreEqual(null, zipkinClient.clientSpan);
        }

        [TestMethod]
        public void StartClientSpan_IsTraceOnIsFalse()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.isTraceOn = false;
            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://" + clientServiceName + ".xyz.net:8000/");

            tracerClient.StartClientTrace(clientServiceUri);

            Assert.AreEqual(null, zipkinClient.serverSpan);
        }

        [TestMethod]
        public void EndClientSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            zipkinClient.clientSpan = new Span();

            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://" + clientServiceName + ".xyz.net:8000/");
            var expectedDuration = fixture.Create<int>();

            tracerClient.EndClientTrace(expectedDuration, clientServiceUri);

            spanTracerStub.AssertWasCalled(x => x.ReceiveClientSpan(zipkinClient.clientSpan, expectedDuration, clientServiceName));
        }

        [TestMethod]
        public void EndClientSpan_UsingIpAddress()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            zipkinClient.clientSpan = new Span();

            var clientServiceName = "192.168.178.178";
            var clientServiceUri = new Uri("https://" + clientServiceName + ":8000/");
            var expectedDuration = fixture.Create<int>();

            tracerClient.EndClientTrace(expectedDuration, clientServiceUri);

            spanTracerStub.AssertWasCalled(x => x.ReceiveClientSpan(zipkinClient.clientSpan, expectedDuration, clientServiceName));
        }

        [TestMethod]
        public void EndClientSpan_MultipleDomainList()
        {
            var zipkinConfig = CreateZipkinConfigWithDefaultValues();
            zipkinConfig.Expect(x => x.GetInternalDomainList()).Return(new List<string>() { ".abc.net", ".xyz.net" });
            var tracerClient = SetupZipkinClient(zipkinConfig);
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            zipkinClient.clientSpan = new Span();

            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://" + clientServiceName + ".xyz.net:8000/");
            var expectedDuration = fixture.Create<int>();

            tracerClient.EndClientTrace(expectedDuration, clientServiceUri);

            spanTracerStub.AssertWasCalled(x => x.ReceiveClientSpan(zipkinClient.clientSpan, expectedDuration, clientServiceName));
        }

        [TestMethod]
        public void EndClientSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            zipkinClient.clientSpan = new Span();

            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://" + clientServiceName + ".xyz.net:8000/");
            var expectedDuration = fixture.Create<int>();

            spanTracerStub.Expect(x => x.ReceiveClientSpan(zipkinClient.clientSpan, expectedDuration, clientServiceName)).Throw(new Exception());

            tracerClient.EndClientTrace(expectedDuration, clientServiceUri);
        }

        [TestMethod]
        public void EndClientSpan_InvalidClientServiceName()
        {
            var tracerClient = SetupZipkinClient();
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            var expectedDuration = fixture.Create<int>();

            tracerClient.EndClientTrace(expectedDuration, null);

            spanTracerStub.AssertWasNotCalled(x => x.ReceiveClientSpan(Arg<Span>.Is.Anything, Arg<int>.Is.Anything, Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void EndClientSpan_IsTraceOnIsFalse_DoesntThrow()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.isTraceOn = false;
            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://" + clientServiceName + ".xyz.net:8000/");

            tracerClient.EndClientTrace(10, clientServiceUri);
        }

        private ITracerClient SetupZipkinClient(IZipkinConfig zipkinConfig = null)
        {
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(MockRepository.GenerateStub<IClientProvider>(), 0);
            spanCollectorBuilder.Expect(x => x.Build(Arg<string>.Is.Anything, Arg<int>.Is.Anything, Arg<int>.Is.Anything)).Return(spanCollectorStub);

            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(true);

            IZipkinConfig zipkinConfigSetup = zipkinConfig;
            if (zipkinConfig == null)
            {
                zipkinConfigSetup = CreateZipkinConfigWithDefaultValues();
            }

            return new ZipkinClient(traceProvider, requestName, logger, zipkinConfigSetup, spanCollectorBuilder);
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
            zipkinConfigStub.Expect(x => x.GetInternalDomainList()).Return(new List<string>() { ".xyz.net" });
            return zipkinConfigStub;
        }

        private IZipkinConfig CreateZipkinConfigWithValues(string zipkinServerName, string zipkinServerPort, string spanProcessorBatchSize, string serviceName, string filterListCsv, string zipkinSampleRate, List<string> domainList = null)
        { 
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(zipkinServerName);
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(zipkinServerPort);
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return(spanProcessorBatchSize);
            zipkinConfigStub.Expect(x => x.ServiceName).Return(serviceName);
            zipkinConfigStub.Expect(x => x.DontSampleListCsv).Return(filterListCsv);
            zipkinConfigStub.Expect(x => x.ZipkinSampleRate).Return(zipkinSampleRate);
            zipkinConfigStub.Expect(x => x.GetInternalDomainList()).Return(domainList);
            return zipkinConfigStub;
        }
    }
}
