using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using System.Collections.Concurrent;
using log4net;

namespace Medidata.ZipkinTracer.Core.Collector.Test
{
    [TestClass]
    public class SpanCollectorTests
    {
        IFixture fixture;
        private IClientProvider clientProviderStub;
        private SpanCollector spanCollector;
        private SpanProcessor spanProcessorStub;
        private ILog logger;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            logger = MockRepository.GenerateStub<ILog>();
        }

        [TestMethod]
        public void GetInstance()
        {
            SpanCollector.spanQueue = null;

            clientProviderStub = MockRepository.GenerateStub<IClientProvider>();
            spanCollector = SpanCollector.GetInstance(clientProviderStub, 0, logger);

            Assert.IsNotNull(SpanCollector.spanQueue);
        }

        [TestMethod]
        public void CTOR_initializesSpanCollector()
        {
            SpanCollector.spanQueue = null;

            clientProviderStub = MockRepository.GenerateStub<IClientProvider>();
            spanCollector = new SpanCollector(clientProviderStub, 0, logger);

            Assert.IsNotNull(SpanCollector.spanQueue);
        }

        [TestMethod]
        public void CTOR_doesntReinitializeSpanCollector()
        {
            var spanQueue = new BlockingCollection<Span>();
            SpanCollector.spanQueue = spanQueue;

            clientProviderStub = MockRepository.GenerateStub<IClientProvider>();
            spanCollector = new SpanCollector(clientProviderStub, 0, logger);

            Assert.IsTrue(System.Object.ReferenceEquals(SpanCollector.spanQueue, spanQueue));
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

            Assert.AreEqual(1, SpanCollector.spanQueue.Count);

            Span queuedSpan;
            var spanInQueue = SpanCollector.spanQueue.TryTake(out queuedSpan);

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
            spanCollector = new SpanCollector(clientProviderStub, 0, logger);

            SpanCollector.spanQueue = fixture.Create<BlockingCollection<Span>>();
            spanProcessorStub = MockRepository.GenerateStub<SpanProcessor>(SpanCollector.spanQueue, clientProviderStub, 0, logger);
            spanCollector.spanProcessor = spanProcessorStub;
        }
    }
}
