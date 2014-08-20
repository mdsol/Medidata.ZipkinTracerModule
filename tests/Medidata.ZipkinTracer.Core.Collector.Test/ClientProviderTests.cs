using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Thrift.Transport;
using System.Collections.Generic;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Core.Collector.Test
{
    [TestClass]
    public class ClientProviderTests
    {
        private Fixture fixture;
        private ClientProvider clientProvider;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            var host = fixture.Create<string>();
            var port = fixture.Create<int>();
            clientProvider = ClientProvider.GetInstance(host, port);
        }

        [TestCleanup]
        public void TearDown()
        {
            ClientProvider.instance = null;
        }

        [TestMethod]
        public void Close()
        {
            Assert.IsNotNull(ClientProvider.instance);

            clientProvider.Close();

            Assert.IsNull(ClientProvider.instance);
        }
    }
}
