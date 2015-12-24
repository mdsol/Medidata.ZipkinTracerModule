using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;

namespace Medidata.ZipkinTracer.Core.Test
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

            var zipkinEndpoint = new ServiceEndpoint();
            var endpoint = zipkinEndpoint.GetLocalEndpoint(serviceName);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(serviceName, endpoint.Service_name);
            Assert.IsNotNull(endpoint.Ipv4);
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
            Assert.AreEqual(serviceName, endpoint.Service_name);
            Assert.IsNotNull(endpoint.Ipv4);
            Assert.IsNotNull(endpoint.Port);
        }
    }
}
