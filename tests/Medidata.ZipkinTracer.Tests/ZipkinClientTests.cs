using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Core.Logging;
using Medidata.ZipkinTracer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class ZipkinClientTests
    {
        private IFixture fixture;
        private SpanCollector spanCollectorStub;
        private SpanTracer spanTracerStub;
        private ITraceProvider traceProvider;
        private ILog logger;
        private HttpContextBase httpContext;
        private NameValueCollection headers;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            traceProvider = MockRepository.GenerateStub<ITraceProvider>();
            logger = MockRepository.GenerateStub<ILog>();
            httpContext = MockRepository.GenerateStub<HttpContextBase>();
            httpContext.Stub(x => x.Items).Return(new Dictionary<string, object>());
            var request = MockRepository.GenerateStub<HttpRequestBase>();
            httpContext.Stub(x => x.Request).Return(request);
            headers = new NameValueCollection();
            request.Stub(x => x.Headers).Return(headers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullConfig()
        {
            new ZipkinClient(null, httpContext);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullContext()
        {
            new ZipkinClient(new ZipkinConfig(), null);
        }

        [TestMethod]
        public void CTOR_WithNullCollector_create_default_collector()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues(sampleRate: 1);

            var client = new ZipkinClient(zipkinConfigStub, httpContext, null);

            Assert.IsNotNull(client.spanCollector);
        }

        [TestMethod]
        public void multiple_Client_WithNullCTORCollector_share_same_collector()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues(sampleRate: 1);

            var client1 = new ZipkinClient(zipkinConfigStub, httpContext, null);
            var client2 = new ZipkinClient(zipkinConfigStub, httpContext, null);

            Assert.IsNotNull(client1.spanCollector);
            Assert.ReferenceEquals(client1.spanCollector, client2.spanCollector);
        }

        [TestMethod]
        public void CTOR_WithTraceIdNullOrEmpty()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            AddTraceId(string.Empty);
            AddSampled(false);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), (uint)0);
            var zipkinClient = new ZipkinClient(zipkinConfigStub, httpContext, spanCollectorStub);
            Assert.IsFalse(zipkinClient.IsTraceOn);
        }

        [TestMethod]
        public void CTOR_WithIsSampledFalse()
        {
            var zipkinConfigStub = CreateZipkinConfigWithDefaultValues();

            AddTraceId(fixture.Create<string>());
            AddSampled(false);

            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), (uint)0);
            var zipkinClient = new ZipkinClient(zipkinConfigStub, httpContext, spanCollectorStub);
            Assert.IsFalse(zipkinClient.IsTraceOn);
        }

        [TestMethod]
        public void CTOR_StartCollector()
        {
            var zipkinClient = (ZipkinClient)SetupZipkinClient();
            Assert.IsNotNull(zipkinClient.spanCollector);
            Assert.IsNotNull(zipkinClient.spanTracer);
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
            spanTracerStub = GetSpanTracerStub();
            zipkinClient.spanTracer = spanTracerStub;
            var uriHost = "https://www.x@y.com";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;
            var requestUri = new Uri(uriHost + uriAbsolutePath);

            var expectedSpan = new Span();
            spanTracerStub.Expect(
                x => x.ReceiveServerSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Equal(traceProvider.TraceId),
                    Arg<string>.Is.Equal(traceProvider.ParentSpanId),
                    Arg<string>.Is.Equal(traceProvider.SpanId),
                    Arg<Uri>.Is.Equal(requestUri))).Return(expectedSpan);

            var result = tracerClient.StartServerTrace(requestUri, methodName);

            Assert.AreEqual(expectedSpan, result);
        }

        [TestMethod]
        public void StartServerSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = GetSpanTracerStub();
            zipkinClient.spanTracer = spanTracerStub;
            var uriHost = "https://www.x@y.com";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;
            var requestUri = new Uri(uriHost + uriAbsolutePath);

            spanTracerStub.Expect(
                x => x.ReceiveServerSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<string>.Is.Anything,
                    Arg<Uri>.Is.Equal(requestUri))).Throw(new Exception());

            var result = tracerClient.StartServerTrace(requestUri, methodName);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void StartServerSpan_IsTraceOnIsFalse()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.IsTraceOn = false;
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
            spanTracerStub = GetSpanTracerStub();
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
            spanTracerStub = GetSpanTracerStub();
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
            zipkinClient.IsTraceOn = false;
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
            spanTracerStub = GetSpanTracerStub();
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "abc-sandbox";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            var expectedSpan = new Span();
            spanTracerStub.Expect(
                x => x.SendClientSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Equal(traceProvider.TraceId),
                    Arg<string>.Is.Equal(traceProvider.ParentSpanId),
                    Arg<string>.Is.Equal(traceProvider.SpanId),
                    Arg<Uri>.Is.Anything)).Return(expectedSpan);

            var result = tracerClient.StartClientTrace(new Uri("https://" + clientServiceName + ".xyz.net:8000" + uriAbsolutePath), methodName, traceProvider);

            Assert.AreEqual(expectedSpan, result);
        }

        [TestMethod]
        public void StartClientSpan_UsingIpAddress()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = GetSpanTracerStub();
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "192.168.178.178";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            var expectedSpan = new Span();
            spanTracerStub.Expect(
                x => x.SendClientSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Equal(traceProvider.TraceId),
                    Arg<string>.Is.Equal(traceProvider.ParentSpanId),
                    Arg<string>.Is.Equal(traceProvider.SpanId),
                    Arg<Uri>.Is.Anything)).Return(expectedSpan);

            var result = tracerClient.StartClientTrace(new Uri("https://" + clientServiceName + ".xyz.net:8000" + uriAbsolutePath), methodName, traceProvider);

            Assert.AreEqual(expectedSpan, result);
        }

        [TestMethod]
        public void StartClientSpan_MultipleDomainList()
        {
            var zipkinConfig = CreateZipkinConfigWithDefaultValues();
            zipkinConfig.NotToBeDisplayedDomainList = new List<string> { ".abc.net", ".xyz.net" };
            var tracerClient = SetupZipkinClient(zipkinConfig);
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = GetSpanTracerStub();
            zipkinClient.spanTracer = spanTracerStub;
            var clientServiceName = "abc-sandbox";
            var uriAbsolutePath = "/object";
            var methodName = "GET";
            var spanName = methodName;

            var expectedSpan = new Span();
            spanTracerStub.Expect(
                x => x.SendClientSpan(
                    Arg<string>.Is.Equal(spanName.ToLower()),
                    Arg<string>.Is.Equal(traceProvider.TraceId),
                    Arg<string>.Is.Equal(traceProvider.ParentSpanId),
                    Arg<string>.Is.Equal(traceProvider.SpanId),
                    Arg<Uri>.Is.Anything)).Return(expectedSpan);

            var result = tracerClient.StartClientTrace(new Uri("https://" + clientServiceName + ".xyz.net:8000" + uriAbsolutePath), methodName, traceProvider);

            Assert.AreEqual(expectedSpan, result);
        }

        [TestMethod]
        public void StartClientSpan_Exception()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = GetSpanTracerStub();
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
                    Arg<Uri>.Is.Anything)).Throw(new Exception());

            var result = tracerClient.StartClientTrace(new Uri("https://" + clientServiceName + ".xyz.net:8000" + uriAbsolutePath), methodName, traceProvider);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void StartClientSpan_IsTraceOnIsFalse()
        {
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.IsTraceOn = false;
            var clientServiceName = "abc-sandbox";
            var clientServiceUri = new Uri("https://" + clientServiceName + ".xyz.net:8000");
            var methodName = "GET";

            var result = tracerClient.StartClientTrace(clientServiceUri, methodName, traceProvider);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void EndClientSpan()
        {
            var returnCode = fixture.Create<short>();
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = GetSpanTracerStub();
            zipkinClient.spanTracer = spanTracerStub;
            var clientSpan = new Span();

            tracerClient.EndClientTrace(clientSpan, returnCode);

            spanTracerStub.AssertWasCalled(x => x.ReceiveClientSpan(clientSpan, returnCode));
        }

        [TestMethod]
        public void EndClientSpan_Exception()
        {
            var returnCode = fixture.Create<short>();
            var tracerClient = SetupZipkinClient();
            var zipkinClient = (ZipkinClient)tracerClient;
            spanTracerStub = GetSpanTracerStub();
            zipkinClient.spanTracer = spanTracerStub;
            var clientSpan = new Span();

            spanTracerStub.Expect(x => x.ReceiveClientSpan(clientSpan, returnCode)).Throw(new Exception());

            tracerClient.EndClientTrace(clientSpan, returnCode);
        }

        [TestMethod]
        public void EndClientSpan_NullClientTrace_DoesntThrow()
        {
            var returnCode = fixture.Create<short>();
            var tracerClient = SetupZipkinClient();
            spanTracerStub = GetSpanTracerStub();

            var called = false;
            spanTracerStub.Stub(x => x.ReceiveClientSpan(Arg<Span>.Is.Anything, Arg<short>.Is.Equal(returnCode)))
                .WhenCalled(x => { called = true; });

            tracerClient.EndClientTrace(null, returnCode);

            Assert.IsFalse(called);
        }

        [TestMethod]
        public void EndClientSpan_IsTraceOnIsFalse_DoesntThrow()
        {
            var returnCode = fixture.Create<short>();
            var tracerClient = SetupZipkinClient();
            spanTracerStub = GetSpanTracerStub();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.IsTraceOn = false;

            var called = false;
            spanTracerStub.Stub(x => x.ReceiveClientSpan(Arg<Span>.Is.Anything, Arg<short>.Is.Equal(returnCode)))
                .WhenCalled(x => { called = true; });

            tracerClient.EndClientTrace(new Span(), returnCode);

            Assert.IsFalse(called);
        }

        [TestMethod]
        [TestCategory("TraceRecordTests")]
        public void Record_IsTraceOnIsFalse_DoesNotAddAnnotation()
        {
            // Arrange
            var tracerClient = SetupZipkinClient();
            spanTracerStub = GetSpanTracerStub();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.IsTraceOn = false;

            var testSpan = new Span();

            // Act
            tracerClient.Record(testSpan, "irrelevant");

            // Assert
            Assert.IsFalse(testSpan.Annotations.Any(), "There are annotations but the trace is off.");
        }

        [TestMethod]
        [TestCategory("TraceRecordTests")]
        public void Record_WithoutValue_AddsAnnotationWithCallerName()
        {
            // Arrange
            var callerMemberName = new StackTrace().GetFrame(0).GetMethod().Name;
            var tracerClient = SetupZipkinClient();
            spanTracerStub = GetSpanTracerStub();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.IsTraceOn = true;

            var testSpan = new Span();

            // Act
            tracerClient.Record(testSpan);

            // Assert
            Assert.AreEqual(1, testSpan.Annotations.Count, "There is not exactly one annotation added.");
            Assert.IsNotNull(
                testSpan.GetAnnotationsByType<Annotation>().SingleOrDefault(a => (string)a.Value == callerMemberName),
                "The record with the caller name is not found in the Annotations."
            );
        }

        [TestMethod]
        [TestCategory("TraceRecordTests")]
        public void RecordBinary_IsTraceOnIsFalse_DoesNotAddBinaryAnnotation()
        {
            // Arrange
            var keyName = "TestKey";
            var testValue = "Some Value";
            var tracerClient = SetupZipkinClient();
            spanTracerStub = GetSpanTracerStub();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.IsTraceOn = false;

            var testSpan = new Span();

            // Act
            tracerClient.RecordBinary(testSpan, keyName, testValue);

            // Assert
            Assert.IsFalse(testSpan.GetAnnotationsByType<Annotation>().Any(), "There are annotations but the trace is off.");
        }

        [TestMethod]
        [TestCategory("TraceRecordTests")]
        public void RecordLocalComponent_WithNotNullValue_AddsLocalComponentAnnotation()
        {
            // Arrange
            var testValue = "Some Value";
            var tracerClient = SetupZipkinClient();
            spanTracerStub = GetSpanTracerStub();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.IsTraceOn = true;

            var testSpan = new Span();

            // Act
            tracerClient.RecordLocalComponent(testSpan, testValue);

            // Assert
            var annotation = testSpan.GetAnnotationsByType<BinaryAnnotation>().SingleOrDefault(a => a.Key == ZipkinConstants.LocalComponent);
            Assert.IsNotNull(annotation, "There is no local trace annotation in the binary annotations.");
            Assert.AreEqual(testValue, annotation.Value, "The local component annotation value is not correct.");
        }

        [TestMethod]
        [TestCategory("TraceRecordTests")]
        public void RecordLocalComponent_IsTraceOnIsFalse_DoesNotAddLocalComponentAnnotation()
        {
            // Arrange
            var testValue = "Some Value";
            var tracerClient = SetupZipkinClient();
            spanTracerStub = GetSpanTracerStub();
            var zipkinClient = (ZipkinClient)tracerClient;
            zipkinClient.IsTraceOn = false;

            var testSpan = new Span();

            // Act
            tracerClient.RecordBinary(testSpan, ZipkinConstants.LocalComponent, testValue);

            // Assert
            Assert.IsFalse(testSpan.GetAnnotationsByType<BinaryAnnotation>().Any(), "There are annotations but the trace is off.");
        }

        private ITracerClient SetupZipkinClient(IZipkinConfig zipkinConfig = null)
        {
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), (uint)0);

            traceProvider.Stub(x => x.TraceId).Return(fixture.Create<string>());
            traceProvider.Stub(x => x.SpanId).Return(fixture.Create<string>());
            traceProvider.Stub(x => x.ParentSpanId).Return(fixture.Create<string>());
            traceProvider.Stub(x => x.IsSampled).Return(true);

            var context = MockRepository.GenerateStub<HttpContextBase>();
            var request = MockRepository.GenerateStub<HttpRequestBase>();
            context.Stub(x => x.Request).Return(request);
            context.Stub(x => x.Items).Return(new Dictionary<string, object> { { TraceProvider.Key, traceProvider } });

            IZipkinConfig zipkinConfigSetup = zipkinConfig;
            if (zipkinConfig == null)
            {
                zipkinConfigSetup = CreateZipkinConfigWithDefaultValues();
            }

            return new ZipkinClient(zipkinConfigSetup, context, spanCollectorStub);
        }

        static readonly char[] separators = new[] { ',', ';' };
        static readonly Func<string, IList<string>> SplitFunc = s => s.Split(separators).Select(e => e.Trim()).ToList();

        private IZipkinConfig CreateZipkinConfigWithDefaultValues(string uriSt = "http://zipkin.com", string domainSt = "http://server.com",
            uint spanProcessorBatchSize = 123, string excludedPathList = "/foo, /bar, /baz", double sampleRate = 0.5, string notToBeDisplayedDomainList = ".xyz.net")
        {
            return new ZipkinConfig
            {
                ZipkinBaseUri = new Uri(uriSt),
                Domain = r => new Uri(domainSt),
                SpanProcessorBatchSize = spanProcessorBatchSize,
                ExcludedPathList = SplitFunc(excludedPathList),
                SampleRate = sampleRate,
                NotToBeDisplayedDomainList = SplitFunc(notToBeDisplayedDomainList),
            };
        }

        private SpanTracer GetSpanTracerStub()
        {
            return
                MockRepository.GenerateStub<SpanTracer>(
                    spanCollectorStub,
                    MockRepository.GenerateStub<ServiceEndpoint>(),
                    new List<string>(),
                    new Uri("http://server.com")
                );
        }

        private void AddTraceId(string traceId)
        {
            headers.Add(TraceProvider.TraceIdHeaderName, traceId);
        }

        private void AddSampled(bool sampled)
        {
            headers.Add(TraceProvider.SampledHeaderName, sampled.ToString());
        }
    }
}
