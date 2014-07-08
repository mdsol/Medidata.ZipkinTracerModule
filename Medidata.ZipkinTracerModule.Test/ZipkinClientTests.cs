using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace Medidata.ZipkinTracerModule.Test
{
    [TestClass]
    public class ZipkinClientTests
    {
        private IFixture fixture;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinServer()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(null);

            var zipkinClient = new ZipkinClient(zipkinConfigStub);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinPort()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(null);

            var zipkinClient = new ZipkinClient(zipkinConfigStub);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CTOR_WithNonIntegerZipkinPort()
        {
            var zipkinConfigStub = MockRepository.GenerateStub<IZipkinConfig>();
            zipkinConfigStub.Expect(x => x.ZipkinServerName).Return(fixture.Create<string>());
            zipkinConfigStub.Expect(x => x.ZipkinServerPort).Return(fixture.Create<string>());

            var zipkinClient = new ZipkinClient(zipkinConfigStub);
        }
    }
}
