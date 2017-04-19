using System;
using System.Collections.Concurrent;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Core.Logging;
using Medidata.ZipkinTracer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class SpanProcessorTests
    {
        private IFixture fixture;
        private SpanProcessor spanProcessor;
        private SpanProcessorTaskFactory taskFactory;
        private BlockingCollection<Span> queue;
        private uint testMaxBatchSize;
        private ILog logger;
 
        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            logger = MockRepository.GenerateStub<ILog>();
            queue = new BlockingCollection<Span>();
            testMaxBatchSize = 10;
            spanProcessor = MockRepository.GenerateStub<SpanProcessor>(new Uri("http://localhost"), queue, testMaxBatchSize);
            spanProcessor.Stub(x => x.SendSpansToZipkin(Arg<string>.Is.Anything)).WhenCalled(s => { });
            taskFactory = MockRepository.GenerateStub<SpanProcessorTaskFactory>(logger, null);
            spanProcessor.spanProcessorTaskFactory = taskFactory;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanQueue()
        {
            new SpanProcessor(new Uri("http://localhost"), null, fixture.Create<uint>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinServer()
        {
            new SpanProcessor(null, queue, fixture.Create<uint>());
        }

        [TestMethod]
        public void Start()
        {
            spanProcessor.Start();
            taskFactory.Expect(x => x.CreateAndStart(Arg<Action>.Matches(y => ValidateStartAction(y, spanProcessor))));
        }

        [TestMethod]
        public void Stop()
        {
            spanProcessor.Stub(x => x.Stop()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            spanProcessor.Stop();
            taskFactory.AssertWasCalled(x => x.StopTask());
        }

        [TestMethod]
        public void Stop_RemainingGetLoggedIfCancelled()
        {
            spanProcessor.Stub(x => x.Stop()).CallOriginalMethod(OriginalCallOptions.NoExpectation);
            taskFactory.Expect(x => x.IsTaskCancelled()).Return(true);

            spanProcessor.spanQueue.Add(GenerateNewSpan());
            spanProcessor.Stop();

            spanProcessor.AssertWasCalled(s => s.SendSpansToZipkin(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void LogSubmittedSpans_DoNotIncrementSubsequentPollCountIfSpanQueueIsEmpty()
        {
            spanProcessor.LogSubmittedSpans();
            Assert.AreEqual(0, spanProcessor.subsequentPollCount);
        }

        [TestMethod]
        public void LogSubmittedSpans_IncrementSubsequentPollCountIfSpanQueueHasAnItemLessThanMax()
        {
            //put item in queue
            spanProcessor.spanQueue.Add(GenerateNewSpan());
            spanProcessor.LogSubmittedSpans();

            //Proces Log with no new items
            spanProcessor.LogSubmittedSpans();

            //Subsquent count has incremented
            Assert.AreEqual(1, spanProcessor.subsequentPollCount);
        }

        [TestMethod]
        public void LogSubmittedSpans_WhenQueueIsSubsequentlyLessThanTheMaxBatchCountMaxTimes()
        {
            spanProcessor.spanQueue.Add(GenerateNewSpan());
            spanProcessor.LogSubmittedSpans();
            spanProcessor.subsequentPollCount = SpanProcessor.MAX_NUMBER_OF_POLLS + 1;
            spanProcessor.LogSubmittedSpans();

            spanProcessor.AssertWasCalled(s => s.SendSpansToZipkin(Arg<string>.Is.Anything));
        }

        [TestMethod]
        public void LogSubmittedSpans_WhenLogEntriesReachMaxBatchSize()
        {
            AddLogEntriesToMaxBatchSize();
            spanProcessor.LogSubmittedSpans();
            spanProcessor.AssertWasCalled( s=> s.SendSpansToZipkin(Arg<string>.Is.Anything));
        }

        private bool ValidateStartAction(Action y, SpanProcessor spanProcessor)
        {
            Assert.AreEqual(() => spanProcessor.LogSubmittedSpans(), y);
            return true;
        }

        private void AddLogEntriesToMaxBatchSize()
        {
            for (int i = 0; i < testMaxBatchSize + 1; i++)
            {
                spanProcessor.spanQueue.Add(GenerateNewSpan());
            }
        }

        private Span GenerateNewSpan()
        {
            return new Span
            {
                Id = fixture.Create<string>(),
                Name = fixture.Create<string>(),
                ParentId = fixture.Create<string>(),
                TraceId = fixture.Create<string>(),
            };
        }
    }
}
