using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using System.Collections.Concurrent;
using Thrift;

namespace Medidata.ZipkinTracerModule.Collector.Test
{
    [TestClass]
    public class SpanCollectorTests
    {
        IFixture fixture;
        private IClientProvider clientProviderStub;
        private SpanCollector spanCollector;
        private SpanProcessor spanProcessorStub;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
        }

        [TestMethod]
        [ExpectedException(typeof(TException))]
        public void CTOR_clientProviderSetupException()
        {
            clientProviderStub = MockRepository.GenerateStub<IClientProvider>();
            clientProviderStub.Expect(x => x.Setup()).Throw(new TException());

            spanCollector = new SpanCollector(clientProviderStub, fixture.Create<int>());
        }

        [TestMethod]
        public void CollectSpans()
        {
            SetupSpanCollector();

            var testSpanId = fixture.Create<long>(); 
            var testTraceId = fixture.Create<long>(); 
            var testParentSpanId = fixture.Create<long>(); 
            var testName = fixture.Create<string>(); 
            
            Span span = new Span();
            span.Id = testSpanId;
            span.Trace_id = testTraceId;
            span.Parent_id = testParentSpanId;
            span.Name = testName;

            spanCollector.Collect(span);

            Assert.AreEqual(1, spanCollector.spanQueue.Count);

            Span queuedSpan;
            var spanInQueue = spanCollector.spanQueue.TryTake(out queuedSpan);

            Assert.AreEqual(span, queuedSpan);
        }

        [TestMethod]
        public void StartProcessingSpans()
        {
            SetupSpanCollector();

            spanCollector.Start();
            spanProcessorStub.AssertWasCalled(x => x.Start());
        }

        [TestMethod]
        public void StopProcessingSpans()
        {
            SetupSpanCollector();

            spanCollector.Stop();

            spanProcessorStub.AssertWasCalled(x => x.Stop());
            clientProviderStub.AssertWasCalled(x => x.Close());
        }

        private void SetupSpanCollector()
        {
            clientProviderStub = MockRepository.GenerateStub<IClientProvider>();
            spanCollector = new SpanCollector(clientProviderStub, 0);

            spanCollector.spanQueue = fixture.Create<BlockingCollection<Span>>();
            spanProcessorStub = MockRepository.GenerateStub<SpanProcessor>(spanCollector.spanQueue, clientProviderStub, 0);
            spanCollector.spanProcessor = spanProcessorStub;
        }
    }
}
