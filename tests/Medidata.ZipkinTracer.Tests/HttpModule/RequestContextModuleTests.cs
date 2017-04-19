using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Medidata.ZipkinTracer.Core.HttpModule;
using Rhino.Mocks;
using System.Diagnostics;
using Medidata.ZipkinTracer.Core.Logging;
using System.Collections.Generic;

namespace Medidata.ZipkinTracer.Core.Test.HttpModule
{
    [TestClass]
    public class RequestContextModuleTests
    {
        private Fixture fixture;
        private IZipkinClient zipkinClient;
        private ZipkinRequestContextModule requestContextModule;
        private IMDLogger logger;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();

            zipkinClient = MockRepository.GenerateStub<IZipkinClient>();
            logger = MockRepository.GenerateStub<IMDLogger>();

            requestContextModule = new ZipkinRequestContextModule();
            requestContextModule.logger = logger;
        }

        [TestMethod]
        public void StartZipkinSpan()
        {
            var url = fixture.Create<string>();
            var traceId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();
            var spanId = fixture.Create<string>();

            var expectedSpan = new Span();

            zipkinClient.Expect(x => x.StartServerSpan(url, traceId, parentSpanId, spanId)).Return(expectedSpan);

            var resultSpan = requestContextModule.StartZipkinSpan(zipkinClient, url, traceId, parentSpanId, spanId);
            Assert.AreEqual(expectedSpan, resultSpan);
        }

        [TestMethod]
        public void StartZipkinSpan_EmptyTraceId()
        {
            var url = fixture.Create<string>();
            string traceId = string.Empty;
            var parentSpanId = fixture.Create<string>();
            var spanId = fixture.Create<string>();

            var resultSpan = requestContextModule.StartZipkinSpan(zipkinClient,url, traceId, parentSpanId, spanId);

            zipkinClient.AssertWasNotCalled(x => x.StartServerSpan(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything));
            logger.AssertWasCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null), options => options.Repeat.Twice());
        }

        [TestMethod]
        public void StartZipkinSpan_EmptySpanId()
        {
            var url = fixture.Create<string>();
            string traceId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();
            var spanId = String.Empty;

            var resultSpan = requestContextModule.StartZipkinSpan(zipkinClient,url, traceId, parentSpanId, spanId);

            zipkinClient.AssertWasNotCalled(x => x.StartServerSpan(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything));
            logger.AssertWasCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null), options => options.Repeat.Twice());
        }

        [TestMethod]
        public void StartZipkinSpan_exception()
        {
            var url = fixture.Create<string>();
            var traceId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();
            var spanId = fixture.Create<string>();

            var exception = new Exception();

            zipkinClient.Expect(x => x.StartServerSpan(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Throw(exception);

            var resultSpan = requestContextModule.StartZipkinSpan(zipkinClient,url, traceId, parentSpanId, spanId);
            Assert.IsNull(resultSpan);

            logger.AssertWasCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null, Arg<Exception>.Is.Equal(exception)));
        }

        [TestMethod]
        public void EndZipkinSpan()
        {
            var span = new Span();
            var stopWatch = new Stopwatch();

            requestContextModule.EndZipkinSpan(zipkinClient,stopWatch, span);

            zipkinClient.AssertWasCalled(x => x.EndServerSpan(Arg<Span>.Is.Equal(span), Arg<int>.Is.Anything));
        }

        [TestMethod]
        public void EndZipkinSpan_Exception()
        {
            var span = new Span();
            var stopWatch = new Stopwatch();

            var exception = new Exception();

            zipkinClient.Expect(x => x.EndServerSpan(Arg<Span>.Is.Anything, Arg<int>.Is.Anything)).Throw(exception);

            requestContextModule.EndZipkinSpan(zipkinClient,stopWatch, span);

            logger.AssertWasCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null, Arg<Exception>.Is.Equal(exception)));
        }

        [TestMethod]
        public void EndZipkinSpan_NullSpan()
        {
            Span span = null;
            var stopWatch = new Stopwatch();

            requestContextModule.EndZipkinSpan(zipkinClient,stopWatch, span);

            zipkinClient.AssertWasNotCalled(x => x.EndServerSpan(Arg<Span>.Is.Anything, Arg<int>.Is.Anything));
            logger.AssertWasNotCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null, Arg<Exception>.Is.Anything));
        }

        [TestMethod]
        public void EndZipkinSpan_NullZipkinClient()
        {
            Span span = new Span();
            var stopWatch = new Stopwatch();

            requestContextModule.EndZipkinSpan(null,stopWatch, span);

            zipkinClient.AssertWasNotCalled(x => x.EndServerSpan(Arg<Span>.Is.Anything, Arg<int>.Is.Anything));
            logger.AssertWasNotCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null, Arg<Exception>.Is.Anything));
        }
    }
}
