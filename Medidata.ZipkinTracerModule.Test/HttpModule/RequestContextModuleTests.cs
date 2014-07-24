using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Medidata.ZipkinTracerModule.HttpModule;
using Rhino.Mocks;
using System.Diagnostics;

namespace Medidata.ZipkinTracerModule.Test.HttpModule
{
    [TestClass]
    public class RequestContextModuleTests
    {
        private Fixture fixture;
        private IZipkinClient zipkinClient;
        private RequestContextModule requestContextModule;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();

            zipkinClient = MockRepository.GenerateStub<IZipkinClient>();
            requestContextModule = new RequestContextModule(zipkinClient);
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
        public void EndZipkinSpan()
        {
            var span = new Span();
            var stopWatch = new Stopwatch();

            requestContextModule.EndZipkinSpan(stopWatch, span);

            zipkinClient.AssertWasCalled(x => x.EndClientSpan(Arg<Span>.Is.Equal(span), Arg<int>.Is.Anything));
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
