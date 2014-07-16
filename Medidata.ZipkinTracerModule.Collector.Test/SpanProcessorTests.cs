using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using System.Collections.Concurrent;
using Rhino.Mocks;
using System.Threading;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule.Collector.Test
{
    [TestClass]
    public class SpanProcessorTests
    {
        private IFixture fixture;
        private SpanProcessor spanProcessor;
        private SpanProcessorTaskFactory taskFactory;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();

            var queue = new BlockingCollection<Span>();
            var clientProvider = MockRepository.GenerateStub<IClientProvider>();
            spanProcessor = new SpanProcessor(queue, clientProvider);
            taskFactory = MockRepository.GenerateStub<SpanProcessorTaskFactory>();
            spanProcessor.spanProcessorTaskFactory = taskFactory;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanQueue()
        {
            var spanProcessor = new SpanProcessor(null, MockRepository.GenerateStub<IClientProvider>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullClientProvider()
        {
            var spanProcessor = new SpanProcessor(new BlockingCollection<Span>(), null);
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

        private bool ValidateStartAction(Action y, SpanProcessor spanProcessor)
        {
            Assert.AreEqual(() => spanProcessor.LogSubmittedSpans(), y);
            return true;
        }
    }
}
