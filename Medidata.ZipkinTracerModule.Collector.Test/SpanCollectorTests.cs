using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using System.Collections.Concurrent;

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

            spanCollector = new SpanCollector();

            spanCollector.spanQueue = fixture.Create<BlockingCollection<Span>>();

            clientProviderStub = MockRepository.GenerateStub<IClientProvider>();
            spanCollector.clientProvider = clientProviderStub; 

            spanProcessorStub = MockRepository.GenerateStub<SpanProcessor>(spanCollector.spanQueue, clientProviderStub);
            spanCollector.spanProcessor = spanProcessorStub;
        }

        [TestMethod]
        public void CollectSpans()
        {
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
            spanCollector.Start();
            spanProcessorStub.AssertWasCalled(x => x.Start());
        }

        [TestMethod]
        public void StopProcessingSpans()
        {
            spanCollector.Stop();

            spanProcessorStub.AssertWasCalled(x => x.Stop());
            clientProviderStub.AssertWasCalled(x => x.Close());
        }
    }
}
