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

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
        }

        [TestMethod]
        public void Start()
        {
            var queue = new BlockingCollection<Span>();
            var clientProvider = MockRepository.GenerateStub<IClientProvider>();
            var spanProcessor = new SpanProcessor(queue, clientProvider);
            var taskFactory = MockRepository.GenerateStub<SpanProcessorTaskFactory>();
            spanProcessor.spanProcessorTaskFactory = taskFactory;

            spanProcessor.Start();

            Action givenAction = null;
            taskFactory.Expect(x => x.CreateAndStart(Arg<Action>.Matches(y => ValidateAction(y, spanProcessor)), Arg<CancellationTokenSource>.Is.Anything));
        }

        private bool ValidateAction(Action y, SpanProcessor spanProcessor)
        {
            Assert.AreEqual(() => spanProcessor.LogSubmittedSpans(), y);
            return true;
        }
    }
}
