using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Medidata.ZipkinTracerModule.HttpModule;
using Rhino.Mocks;
using System.Diagnostics;
using Medidata.ZipkinTracerModule.Logging;
using System.Collections.Generic;

namespace Medidata.ZipkinTracerModule.Test.HttpModule
{
    [TestClass]
    public class RequestContextModuleTests
    {
        private Fixture fixture;
        private IZipkinClient zipkinClient;
        private RequestContextModule requestContextModule;
        private IMDLogger logger;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();

            zipkinClient = MockRepository.GenerateStub<IZipkinClient>();
            logger = MockRepository.GenerateStub<IMDLogger>();

            requestContextModule = new RequestContextModule(zipkinClient, logger);
        }

        [TestMethod]
        public void StartZipkinSpan()
        {
            var url = fixture.Create<string>();
            var traceId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();

            var expectedSpan = new Span();

            zipkinClient.Expect(x => x.StartClientSpan(url, traceId, parentSpanId)).Return(expectedSpan);

            var resultSpan = requestContextModule.StartZipkinSpan(url, traceId, parentSpanId);
            Assert.AreEqual(expectedSpan, resultSpan);
        }

        [TestMethod]
        public void StartZipkinSpan_nullTraceId()
        {
            var url = fixture.Create<string>();
            string traceId = null;
            var parentSpanId = fixture.Create<string>();

            var resultSpan = requestContextModule.StartZipkinSpan(url, traceId, parentSpanId);

            zipkinClient.AssertWasNotCalled(x => x.StartClientSpan(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything));
            logger.AssertWasCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null));
        }

        [TestMethod]
        public void StartZipkinSpan_exception()
        {
            var url = fixture.Create<string>();
            var traceId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>();

            var exception = new Exception();

            zipkinClient.Expect(x => x.StartClientSpan(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Throw(exception);

            var resultSpan = requestContextModule.StartZipkinSpan(url, traceId, parentSpanId);
            Assert.IsNull(resultSpan);

            logger.AssertWasCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null, Arg<Exception>.Is.Equal(exception)));
        }

        [TestMethod]
        public void EndZipkinSpan()
        {
            var span = new Span();
            var stopWatch = new Stopwatch();

            requestContextModule.EndZipkinSpan(stopWatch, span);

            zipkinClient.AssertWasCalled(x => x.EndClientSpan(Arg<Span>.Is.Equal(span), Arg<int>.Is.Anything));
        }

        [TestMethod]
        public void EndZipkinSpan_Exception()
        {
            var span = new Span();
            var stopWatch = new Stopwatch();

            var exception = new Exception();

            zipkinClient.Expect(x => x.EndClientSpan(Arg<Span>.Is.Anything, Arg<int>.Is.Anything)).Throw(exception);

            requestContextModule.EndZipkinSpan(stopWatch, span);

            logger.AssertWasCalled(x => x.Event(Arg<string>.Is.Anything, Arg<HashSet<string>>.Is.Null, Arg<object>.Is.Null, Arg<Exception>.Is.Equal(exception)));
        }

        [TestMethod]
        public void EndZipkinSpan_NullSpan()
        {
            Span span = null;
            var stopWatch = new Stopwatch();

            requestContextModule.EndZipkinSpan(stopWatch, span);

            zipkinClient.AssertWasNotCalled(x => x.EndClientSpan(Arg<Span>.Is.Anything, Arg<int>.Is.Anything));
        }
    }
}
