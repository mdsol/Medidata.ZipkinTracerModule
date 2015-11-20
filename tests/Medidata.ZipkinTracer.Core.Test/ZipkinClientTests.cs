using System;
using System.Collections.Generic;
using System.Configuration;
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
        private ITraceProvider traceProvider;
        private ITraceProvider nextTraceProvider;
        private ILog logger;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanCollectorBuilder = MockRepository.GenerateStub<ISpanCollectorBuilder>();
            traceProvider = MockRepository.GenerateStub<ITraceProvider>();
            nextTraceProvider = MockRepository.GenerateStub<ITraceProvider>();
            logger = MockRepository.GenerateStub<ILog>();
        }

        [TestMethod]
        public void CTOR_WithNullLogger()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", fixture.Create<string>(), "goo,bar", "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullZipkinServer()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(null, "123", fixture.Create<string>(), "goo,bar", "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullSpanProcessorBatchSize()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), null, fixture.Create<string>(), "goo,bar", "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullServiceName()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", null, "goo,bar", "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullWhiteListCsv()
        {
            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(true);

            var zipkinConfigStub = CreateZipkinConfigWithValues("http://localhost", "123", fixture.Create<string>(), null, "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsTrue(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithNullZipkinSampleRate()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "123", fixture.Create<string>(), "asfdsa", null);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNonIntegerSpanProcessorBatchSize()
        {
            var zipkinConfigStub = CreateZipkinConfigWithValues(fixture.Create<string>(), "sfa", fixture.Create<string>(), "goo,bar", "0.5");

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);

            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithNullTraceProvider()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(null, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
            logger.AssertWasCalled(x => x.Error(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void CTOR_WithTraceIdNullOrEmpty()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            traceProvider.Expect(x => x.TraceId).Return(string.Empty);
            traceProvider.Expect(x => x.IsSampled).Return(true);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_WithIsSampledFalse()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(false);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);
            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void CTOR_StartCollector()
        {
            var zipkinClient = (ZipkinClient)SetupZipkinClient();
            Assert.IsNotNull(zipkinClient.spanCollector);
            Assert.IsNotNull(zipkinClient.spanTracer);
        }

        [TestMethod]
        public void CTOR_ZpkinServer_Exception()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.IsSampled).Return(true);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);

            var expectedException = new Exception();
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Throw(expectedException);

            var zipkinClient = new ZipkinClient(traceProvider, logger, zipkinConfigStub, spanCollectorBuilder);
            Assert.IsFalse(zipkinClient.isTraceOn);
        }

        [TestMethod]
        public void Shutdown_StopCollector()
        {
            var zipkinClient = (ZipkinClient)SetupZipkinClient();

            zipkinClient.ShutDown();

            spanCollectorStub.AssertWasCalled(x => x.Stop());
        }

        [TestMethod]
        public void Shutdown_CollectorNullDoesntThrow()
        {
            var zipkinClient = (ZipkinClient)SetupZipkinClient();
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
            var uriHost = "https://www.x@y.com";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            var expectedSpan = new Span();
            spanTracerStub.Expect(
                x => x.ReceiveServerSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Equal(traceProvider.TraceId),
                    Arg<string>.Is.Equal(traceProvider.ParentSpanId),
                    Arg<string>.Is.Equal(traceProvider.SpanId),
                    Arg<string>.Is.Anything)).Return(expectedSpan);

            var result = tracerClient.StartServerTrace(new Uri(uriHost + uriAbsolutePath), methodName);

            Assert.AreEqual(expectedSpan, result);
        }

        [TestMethod]
        public void StartServerSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var uriHost = "https://www.x@y.com";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            spanTracerStub.Expect(
                x => x.ReceiveServerSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything)).Throw(new Exception());

            var result = tracerClient.StartServerTrace(new Uri(uriHost + uriAbsolutePath), methodName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void StartServerSpan_IsTraceOnIsFalse()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.isTraceOn = false;
            var uriHost = "https://www.x@y.com";
            var uriAbsolutePath = "/object";
            var methodName = "GET";

            var result = tracerClient.StartServerTrace(new Uri(uriHost + uriAbsolutePath), methodName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void EndServerSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var serverSpan = new Span();

            tracerClient.EndServerTrace(serverSpan);

            spanTracerStub.AssertWasCalled(x => x.SendServerSpan(serverSpan));
        }

        [TestMethod]
        public void EndServerSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var serverSpan = new Span();

            spanTracerStub.Expect(x => x.SendServerSpan(serverSpan)).Throw(new Exception());

            tracerClient.EndServerTrace(serverSpan);
        }

        [TestMethod]
        public void EndServerSpan_IsTraceOnIsFalse_DoesntThrow()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.isTraceOn = false;
            var serverSpan = new Span();

            tracerClient.EndServerTrace(serverSpan);
        }

        [TestMethod]
        public void EndServerSpan_NullServerSpan_DoesntThrow()
        {
            var tracerClient = SetupZipkinClient();

            tracerClient.EndServerTrace(null);
        }

        [TestMethod]
        public void StartClientSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "abc-sandbox";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            var expectedSpan = new Span();
            spanTracerStub.Expect(
                x => x.SendClientSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Equal(nextTraceProvider.TraceId),
                    Arg<string>.Is.Equal(nextTraceProvider.ParentSpanId),
                    Arg<string>.Is.Equal(nextTraceProvider.SpanId),
                    Arg<string>.Is.Equal(clientServiceName),
                    Arg<string>.Is.Anything)).Return(expectedSpan);

            var result = tracerClient.StartClientTrace(new Uri("https://" + clientServiceName + ".xyz.net:8000" + uriAbsolutePath), methodName);

            Assert.AreEqual(expectedSpan, result);
        }

        [TestMethod]
        public void StartClientSpan_UsingIpAddress()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "192.168.178.178";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            var expectedSpan = new Span();
            spanTracerStub.Expect(
                x => x.SendClientSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Equal(nextTraceProvider.TraceId),
                    Arg<string>.Is.Equal(nextTraceProvider.ParentSpanId),
                    Arg<string>.Is.Equal(nextTraceProvider.SpanId),
                    Arg<string>.Is.Equal(clientServiceName),
                    Arg<string>.Is.Anything)).Return(expectedSpan);

            var result = tracerClient.StartClientTrace(new Uri("https://" + clientServiceName + ".xyz.net:8000" + uriAbsolutePath), methodName);

            Assert.AreEqual(expectedSpan, result);
        }

        [TestMethod]
        public void StartClientSpan_MultipleDomainList()
        {
            var zipkinConfig = CreateZipkinConfigWithDefaultValues();
            zipkinConfig.Expect(x => x.GetNotToBeDisplayedDomainList()).Return(new List<string>() { ".abc.net", ".xyz.net" });
            var tracerClient = SetupZipkinClient(zipkinConfig);
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "abc-sandbox";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            var expectedSpan = new Span();
            spanTracerStub.Expect(
                x => x.SendClientSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Equal(nextTraceProvider.TraceId),
                    Arg<string>.Is.Equal(nextTraceProvider.ParentSpanId),
                    Arg<string>.Is.Equal(nextTraceProvider.SpanId),
                    Arg<string>.Is.Equal(clientServiceName),
                    Arg<string>.Is.Anything)).Return(expectedSpan);

            var result = tracerClient.StartClientTrace(new Uri("https://" + clientServiceName + ".xyz.net:8000" + uriAbsolutePath), methodName);

            Assert.AreEqual(expectedSpan, result);
        }

        [TestMethod]
        public void StartClientSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "abc-sandbox";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            spanTracerStub.Expect(
                x => x.SendClientSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Equal(clientServiceName),
                    Arg<string>.Is.Anything)).Throw(new Exception());

            var result = tracerClient.StartClientTrace(new Uri("https://" + clientServiceName + ".xyz.net:8000" + uriAbsolutePath), methodName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void StartClientSpan_InvalidClientServiceName()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var methodName = "GET";

            var result = tracerClient.StartClientTrace(null, methodName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void StartClientSpan_IsTraceOnIsFalse()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.isTraceOn = false;
            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://" + clientServiceName + ".xyz.net:8000");
            var methodName = "GET";

            var result = tracerClient.StartClientTrace(clientServiceUri, methodName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void EndClientSpan()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientSpan = new Span();

            tracerClient.EndClientTrace(clientSpan);

            spanTracerStub.AssertWasCalled(x => x.ReceiveClientSpan(clientSpan));
        }

        [TestMethod]
        public void EndClientSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            zipkinClient.spanTracer = spanTracerStub;
            var clientSpan = new Span();

            spanTracerStub.Expect(x => x.ReceiveClientSpan(clientSpan)).Throw(new Exception());

            tracerClient.EndClientTrace(clientSpan);
        }

        [TestMethod]
        public void EndClientSpan_NullClientTrace_DoesntThrow()
        {
            var tracerClient = SetupZipkinClient();
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());

            var called = false;
            spanTracerStub.Stub(x => x.ReceiveClientSpan(Arg<Span>.Is.Anything))
                .WhenCalled(x => { called = true; });

            tracerClient.EndClientTrace(null);

            Assert.IsFalse(called);
        }

        [TestMethod]
        public void EndClientSpan_IsTraceOnIsFalse_DoesntThrow()
        {
            var tracerClient = SetupZipkinClient();
            spanTracerStub = MockRepository.GenerateStub<SpanTracer>(spanCollectorStub, fixture.Create<string>(), MockRepository.GenerateStub<ServiceEndpoint>());
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.isTraceOn = false;

            var called = false;
            spanTracerStub.Stub(x => x.ReceiveClientSpan(Arg<Span>.Is.Anything))
                .WhenCalled(x => { called = true; });

            tracerClient.EndClientTrace(new Span());

            Assert.IsFalse(called);
        }

        private ITracerClient SetupZipkinClient(IZipkinConfig zipkinConfig = null)
        {
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            spanCollectorBuilder.Expect(x => x.Build(Arg<Uri>.Is.Anything, Arg<int>.Is.Anything, Arg<ILog>.Is.Equal(logger))).Return(spanCollectorStub);

            traceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.SpanId).Return(fixture.Create<string>());
            traceProvider.Expect(x => x.ParentSpanId).Return(traceProvider.TraceId);
            traceProvider.Expect(x => x.IsSampled).Return(true);
            
            nextTraceProvider.Expect(x => x.TraceId).Return(fixture.Create<string>());
            nextTraceProvider.Expect(x => x.SpanId).Return(fixture.Create<string>());
            nextTraceProvider.Expect(x => x.ParentSpanId).Return(traceProvider.TraceId);
            nextTraceProvider.Expect(x => x.IsSampled).Return(true);
            traceProvider.Stub(x => x.GetNext()).Return(nextTraceProvider);

            IZipkinConfig zipkinConfigSetup = zipkinConfig;
            if (zipkinConfig == null)
            {
                zipkinConfigSetup = CreateZipkinConfigWithDefaultValues();
            }

            return new ZipkinClient(traceProvider, logger, zipkinConfigSetup, spanCollectorBuilder);
        }

        private IZipkinConfig CreateZipkinConfigWithDefaultValues()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinBaseUri).Return("http://localhost");
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return("123");
            zipkinConfigStub.Expect(x => x.ServiceName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.DontSampleListCsv).Return("foo,bar,baz");
            zipkinConfigStub.Expect(x => x.ZipkinSampleRate).Return("0.5");
            zipkinConfigStub.Expect(x => x.GetNotToBeDisplayedDomainList()).Return(new List<string>() { ".xyz.net" });
            return zipkinConfigStub;
        }

        private IZipkinConfig CreateZipkinConfigWithValues(string zipkinServerName, string spanProcessorBatchSize, string serviceName, string filterListCsv, string zipkinSampleRate, List<string> domainList = null, Uri proxyUri = null, string proxyType = null)
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinBaseUri).Return(zipkinServerName);
            zipkinConfigStub.Expect(x => x.SpanProcessorBatchSize).Return(spanProcessorBatchSize);
            zipkinConfigStub.Expect(x => x.ServiceName).Return(serviceName);
            zipkinConfigStub.Expect(x => x.DontSampleListCsv).Return(filterListCsv);
            zipkinConfigStub.Expect(x => x.ZipkinSampleRate).Return(zipkinSampleRate);
            zipkinConfigStub.Expect(x => x.GetNotToBeDisplayedDomainList()).Return(domainList);
            return zipkinConfigStub;
        }
    }
}
