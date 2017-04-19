using System;
using System.Threading;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Core.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class SpanProcesssorTaskFactoryTests
    {
        private SpanProcessorTaskFactory spanProcessorTaskFactory;
        private CancellationTokenSource cancellationTokenSource;
        private bool actionCalled;
        private ILog logger;

        [TestInitialize]
        public void Init()
        {
            logger = MockRepository.GenerateStub<ILog>();
            cancellationTokenSource = new CancellationTokenSource();
            spanProcessorTaskFactory = new SpanProcessorTaskFactory(logger, cancellationTokenSource);
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
        public void ActionWrapper_Exception()
        {
            Exception ex = new Exception("Exception!");
            bool logErrorCalled = false;
            logger.Stub(x => x.Log(Arg<LogLevel>.Is.Equal(LogLevel.Error), Arg<Func<string>>.Is.Null, Arg<Exception>.Is.Null, Arg<object>.Is.Null)).Return(true);
            logger.Stub(x => x.ErrorException("Error in SpanProcessorTask", ex))
                .WhenCalled(x => { logErrorCalled = true; }).Return(true);
            var myAction = new Action(() => { actionCalled = true; throw ex; });
            Assert.IsFalse(actionCalled);

            spanProcessorTaskFactory.ActionWrapper(myAction);
            Assert.IsTrue(actionCalled);
            Assert.IsTrue(logErrorCalled);

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
