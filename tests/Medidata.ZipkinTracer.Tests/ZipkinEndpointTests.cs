using System;
using Medidata.ZipkinTracer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class ZipkinEndpointTests
    {
        private IFixture fixture;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
        }

        [TestMethod]
        public void GetLocalEndpoint()
        {
            var serviceName = fixture.Create<string>();
            var port = fixture.Create<ushort>();

            var zipkinEndpoint = new ServiceEndpoint();
            var endpoint = zipkinEndpoint.GetLocalEndpoint(serviceName, port);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(serviceName, endpoint.ServiceName);
            Assert.IsNotNull(endpoint.IPAddress);
            Assert.IsNotNull(endpoint.Port);
        }

        [TestMethod]
        public void GetRemoteEndpoint()
        {
            var remoteUri = new Uri("http://localhost");
            var serviceName = fixture.Create<string>();

            var zipkinEndpoint = new ServiceEndpoint();
            var endpoint = zipkinEndpoint.GetRemoteEndpoint(remoteUri, serviceName);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(serviceName, endpoint.ServiceName);
            Assert.IsNotNull(endpoint.IPAddress);
            Assert.IsNotNull(endpoint.Port);
        }
    }
}
