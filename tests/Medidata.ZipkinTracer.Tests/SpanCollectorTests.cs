using System;
using System.Collections.Concurrent;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Core.Logging;
using Medidata.ZipkinTracer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class SpanCollectorTests
    {
        IFixture fixture;
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
        public void CTOR_initializesSpanCollector()
        {
            SpanCollector.spanQueue = null;

            spanCollector = new SpanCollector(new Uri("http://localhost"), 0);

            Assert.IsNotNull(SpanCollector.spanQueue);
        }

        [TestMethod]
        public void CTOR_doesntReinitializeSpanCollector()
        {
            var spanQueue = new BlockingCollection<Span>();
            SpanCollector.spanQueue = spanQueue;

            spanCollector = new SpanCollector(new Uri("http://localhost"), 0);

            Assert.IsTrue(System.Object.ReferenceEquals(SpanCollector.spanQueue, spanQueue));
        }

        [TestMethod]
        public void CollectSpans()
        {
            SetupSpanCollector();

            var testSpanId = fixture.Create<string>(); 
            var testTraceId = fixture.Create<string>(); 
            var testParentSpanId = fixture.Create<string>(); 
            var testName = fixture.Create<string>();
            
            Span span = new Span();
            span.Id = testSpanId;
            span.TraceId = testTraceId;
            span.ParentId = testParentSpanId;
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
            Assert.IsTrue(spanCollector.IsStarted);
        }

        [TestMethod]
        public void StopProcessingSpansWithoutStartFirst()
        {
            SetupSpanCollector();

            spanCollector.Stop();

            spanProcessorStub.AssertWasNotCalled(x => x.Stop());
            Assert.IsFalse(spanCollector.IsStarted);
        }

        [TestMethod]
        public void StopProcessingSpans()
        {
            SetupSpanCollector();

            spanCollector.Start();
            spanCollector.Stop();

            spanProcessorStub.AssertWasCalled(x => x.Stop());
            Assert.IsFalse(spanCollector.IsStarted);
        }

        private void SetupSpanCollector()
        {
            spanCollector = new SpanCollector(new Uri("http://localhost"), 0);

            SpanCollector.spanQueue = fixture.Create<BlockingCollection<Span>>();
            spanProcessorStub = MockRepository.GenerateStub<SpanProcessor>(new Uri("http://localhost"),
                    SpanCollector.spanQueue, (uint)0);
            spanCollector.spanProcessor = spanProcessorStub;
        }
    }
}
