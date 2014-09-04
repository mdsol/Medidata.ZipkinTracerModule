using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using System.Threading;

namespace Medidata.ZipkinTracer.Core.Collector.Test
{
    [TestClass]
    public class SpanProcesssorTaskFactoryTests
    {
        private Fixture fixture;
        private SpanProcessorTaskFactory spanProcessorTaskFactory;
        private CancellationTokenSource cancellationTokenSource;
        private bool actionCalled;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            spanProcessorTaskFactory = new SpanProcessorTaskFactory();
            cancellationTokenSource = new CancellationTokenSource();
            spanProcessorTaskFactory.cancellationTokenSource = cancellationTokenSource;
            actionCalled = false;
        }

        [TestMethod]
        public void StopTask()
        {
            Assert.IsFalse(cancellationTokenSource.IsCancellationRequested);

            spanProcessorTaskFactory.StopTask();

            Assert.IsTrue(cancellationTokenSource.IsCancellationRequested);
        }

        [TestMethod]
        public void IsTaskCancelled()
        {
            Assert.IsFalse(cancellationTokenSource.IsCancellationRequested);
            Assert.IsFalse(spanProcessorTaskFactory.IsTaskCancelled());

            cancellationTokenSource.Cancel();
            Assert.IsTrue(cancellationTokenSource.IsCancellationRequested);
            Assert.IsTrue(spanProcessorTaskFactory.IsTaskCancelled());
        }

        [TestMethod]
        public void ActionWrapper()
        {
            var myAction = new Action(() => { actionCalled = true; });
            Assert.IsFalse(actionCalled);

            spanProcessorTaskFactory.ActionWrapper(myAction);
            Assert.IsTrue(actionCalled);

            cancellationTokenSource.Cancel();
        }

        [TestMethod]
        public void ActionWrapper_NotCalledIfCancelled()
        {
            var myAction = new Action(() => { actionCalled = true; });
            Assert.IsFalse(actionCalled);

            cancellationTokenSource.Cancel();
            spanProcessorTaskFactory.ActionWrapper(myAction);
            Assert.IsFalse(actionCalled);
        }
    }
}
