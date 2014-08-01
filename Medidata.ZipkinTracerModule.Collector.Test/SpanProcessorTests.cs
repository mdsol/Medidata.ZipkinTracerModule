using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using System.Collections.Concurrent;
using Rhino.Mocks;
using System.Threading;
using System.Threading.Tasks;
using Thrift;
using System.Collections.Generic;

namespace Medidata.ZipkinTracerModule.Collector.Test
{
    [TestClass]
    public class SpanProcessorTests
    {
        private IFixture fixture;
        private SpanProcessor spanProcessor;
        private SpanProcessorTaskFactory taskFactory;
        private IClientProvider clientProvider;
        private BlockingCollection<Span> queue;
        private int testMaxBatchSize;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();

            queue = new BlockingCollection<Span>();
            clientProvider = MockRepository.GenerateStub<IClientProvider>();
            testMaxBatchSize = 10;
            spanProcessor = new SpanProcessor(queue, clientProvider, testMaxBatchSize);
            taskFactory = MockRepository.GenerateStub<SpanProcessorTaskFactory>();
            spanProcessor.spanProcessorTaskFactory = taskFactory;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanQueue()
        {
            var spanProcessor = new SpanProcessor(null, MockRepository.GenerateStub<IClientProvider>(), fixture.Create<int>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullClientProvider()
        {
            var spanProcessor = new SpanProcessor(new BlockingCollection<Span>(), null, fixture.Create<int>());
        }

        [TestMethod]
        public void Start()
        {
            spanProcessor.Start();
            taskFactory.Expect(x => x.CreateAndStart(Arg<Action>.Matches(y => ValidateStartAction(y, spanProcessor)), Arg<CancellationTokenSource>.Is.Anything));
        }

        [TestMethod]
        public void Stop()
        {
            spanProcessor.cancellationTokenSource = new CancellationTokenSource();
            spanProcessor.Stop();

            Assert.IsTrue(spanProcessor.cancellationTokenSource.Token.IsCancellationRequested);
        }

        [TestMethod]
        public void Stop_RemainingGetLoggedIfCancelled()
        {
            spanProcessor.cancellationTokenSource = new CancellationTokenSource();
            spanProcessor.spanQueue.Add(new Span());
            spanProcessor.Stop();

            clientProvider.AssertWasCalled(x => x.Log(Arg<List<LogEntry>>.Is.Anything));
        }

        [TestMethod]
        public void Log_HandleThriftExceptionRetryIfLessThan3Tries()
        {
            spanProcessor.retries = 2;
            clientProvider.Expect(x => x.Log(Arg<List<LogEntry>>.Is.Anything)).Throw(new TException());
            spanProcessor.Log(clientProvider, new List<LogEntry>());
        }

        [TestMethod]
        [ExpectedException(typeof(TException))]
        public void Log_HandleThriftExceptionOnThirdTry()
        {
            spanProcessor.retries = 3;
            clientProvider.Expect(x => x.Log(Arg<List<LogEntry>>.Is.Anything)).Throw(new TException());
            spanProcessor.Log(clientProvider, new List<LogEntry>());
        }

        [TestMethod]
        public void LogSubmittedSpans_IncrementSubsequentEmptyQueueCountIfSpanQueueEmpty()
        {
            spanProcessor.LogSubmittedSpans();
            Assert.AreEqual(1, spanProcessor.subsequentEmptyQueueCount);
        }

        [TestMethod]
        public void LogSubmittedSpans_WhenQueueIsSubsequentlyEmptyForMaxTimes()
        {
            spanProcessor.cancellationTokenSource = new CancellationTokenSource();
            spanProcessor.subsequentEmptyQueueCount = SpanProcessor.MAX_SUBSEQUENT_EMPTY_QUEUE + 1;
            spanProcessor.logEntries.Add(new LogEntry());
            spanProcessor.LogSubmittedSpans();

            clientProvider.AssertWasCalled(x => x.Log(Arg<List<LogEntry>>.Is.Anything));
        }

        [TestMethod]
        public void LogSubmittedSpans_WhenLogEntriesReachMaxBatchSize()
        {
            spanProcessor.cancellationTokenSource = new CancellationTokenSource();
            AddLogEntriesToMaxBatchSize();
            spanProcessor.LogSubmittedSpans();

            clientProvider.AssertWasCalled(x => x.Log(Arg<List<LogEntry>>.Is.Anything));
        }


        private bool ValidateStartAction(Action y, SpanProcessor spanProcessor)
        {
            Assert.AreEqual(() => spanProcessor.LogSubmittedSpansWrapper(), y);
            return true;
        }

        private void AddLogEntriesToMaxBatchSize()
        {
            for (int i = 0; i < testMaxBatchSize + 1; i++)
            {
                spanProcessor.logEntries.Add(new LogEntry());
            }
        }
    }
}
