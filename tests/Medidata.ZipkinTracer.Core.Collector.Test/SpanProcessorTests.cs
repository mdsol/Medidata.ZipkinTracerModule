using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using System.Collections.Concurrent;
using Rhino.Mocks;
using Thrift;
using System.Collections.Generic;
using log4net;

namespace Medidata.ZipkinTracer.Core.Collector.Test
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
        private ILog logger;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            logger = MockRepository.GenerateStub<ILog>();
            queue = new BlockingCollection<Span>();
            clientProvider = MockRepository.GenerateStub<IClientProvider>();
            testMaxBatchSize = 10;
            spanProcessor = new SpanProcessor(queue, clientProvider, testMaxBatchSize, logger);
            taskFactory = MockRepository.GenerateStub<SpanProcessorTaskFactory>(logger, null);
            spanProcessor.spanProcessorTaskFactory = taskFactory;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanQueue()
        {
            new SpanProcessor(null, clientProvider, fixture.Create<int>(), logger);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullClientProvider()
        {
            new SpanProcessor(new BlockingCollection<Span>(), null, fixture.Create<int>(), logger);
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
            spanProcessor.Stop();
            taskFactory.AssertWasCalled(x => x.StopTask());
        }

        [TestMethod]
        public void Stop_RemainingGetLoggedIfCancelled()
        {
            taskFactory.Expect(x => x.IsTaskCancelled()).Return(true);

            spanProcessor.spanQueue.Add(new Span());
            spanProcessor.Stop();

            clientProvider.AssertWasCalled(x => x.Log(Arg<List<LogEntry>>.Is.Anything));
        }

        [TestMethod]
        public void Log_HandleThriftExceptionRetryIfLessThan3Tries()
        {
            spanProcessor.retries = 2;
            clientProvider.Expect(x => x.Log(Arg<List<LogEntry>>.Is.Anything)).Throw(new TException()).Repeat.Once();
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
        public void LogSubmittedSpans_DoNotIncrementSubsequentPollCountIfSpanQueueIsEmpty()
        {
            spanProcessor.LogSubmittedSpans();
            Assert.AreEqual(0, spanProcessor.subsequentPollCount);
        }

        [TestMethod]
        public void LogSubmittedSpans_IncrementSubsequentPollCountIfSpanQueueHasAnItemLessThanMax()
        {
            spanProcessor.logEntries.Add(new LogEntry());
            spanProcessor.LogSubmittedSpans();
            Assert.AreEqual(1, spanProcessor.subsequentPollCount);
        }

        [TestMethod]
        public void LogSubmittedSpans_WhenQueueIsSubsequentlyLessThanTheMaxBatchCountMaxTimes()
        {
            spanProcessor.subsequentPollCount = SpanProcessor.MAX_NUMBER_OF_POLLS + 1;
            spanProcessor.logEntries.Add(new LogEntry());
            spanProcessor.LogSubmittedSpans();

            clientProvider.AssertWasCalled(x => x.Log(Arg<List<LogEntry>>.Is.Anything));
        }

        [TestMethod]
        public void LogSubmittedSpans_WhenLogEntriesReachMaxBatchSize()
        {
            AddLogEntriesToMaxBatchSize();
            spanProcessor.LogSubmittedSpans();

            clientProvider.AssertWasCalled(x => x.Log(Arg<List<LogEntry>>.Is.Anything));
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
                spanProcessor.logEntries.Add(new LogEntry());
            }
        }
    }
}
