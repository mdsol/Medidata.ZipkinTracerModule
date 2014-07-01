using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace Medidata.ZipkinTracerModule.Test
{
    [TestClass]
    public class SpanCollectorTests
    {
        IFixture fixture;
        private IClientProvider clientProvider;
        private SpanCollector spanCollector;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();

            clientProvider = MockRepository.GenerateStub<IClientProvider>();
            spanCollector = new SpanCollector(clientProvider);
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

        }
    }
}
