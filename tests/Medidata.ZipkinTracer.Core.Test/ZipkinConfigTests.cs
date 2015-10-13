using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Net;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class ZipkinConfigTests
    {
        #region Get Not To Be Displayed Domain List
        [TestMethod]
        public void GetNotToBeDisplayedDomainList()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            ConfigurationManager.AppSettings["zipkinNotToBeDisplayedDomainList"] = ".xyz.net,.abc.com";

            // Act
            var result = config.GetNotToBeDisplayedDomainList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(".xyz.net", result[0]);
            Assert.AreEqual(".abc.com", result[1]);
        }

        [TestMethod]
        public void GetNotToBeDisplayedDomainListWithEmptyConfig()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            ConfigurationManager.AppSettings["zipkinNotToBeDisplayedDomainList"] = "";

            // Act
            var result = config.GetNotToBeDisplayedDomainList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetNotToBeDisplayedDomainListWithOnlyCommaLocalesConfigValues()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            ConfigurationManager.AppSettings["zipkinNotToBeDisplayedDomainList"] = ",";

            // Act
            var result = config.GetNotToBeDisplayedDomainList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
        #endregion

        [TestMethod]
        public void ZipkinProxyServer()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            Uri proxy = new Uri("http://localhost:8888");
            WebRequest.DefaultWebProxy = new WebProxy(proxy, false);
            ConfigurationManager.AppSettings["zipkinScribeServerName"] = "abc.xyz.com";
            ConfigurationManager.AppSettings["zipkinScribeServerPort"] = "9410";

            // Act
            var result = config.ZipkinProxyServer;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(proxy, result);
        }

        [TestMethod]
        public void ZipkinProxyServerNullProxyServer()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            WebRequest.DefaultWebProxy = null;

            // Act
            var result = config.ZipkinProxyServer;

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ZipkinProxyServerByPassList()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            Uri proxy = new Uri("http://localhost:8888");
            WebRequest.DefaultWebProxy = new WebProxy(proxy, false, new[] { "abc.xyz.com" });
            ConfigurationManager.AppSettings["zipkinScribeServerName"] = "abc.xyz.com";
            ConfigurationManager.AppSettings["zipkinScribeServerPort"] = "9410";

            // Act
            var result = config.ZipkinProxyServer;

            // Assert
            Assert.IsNull(result);
        }
    }
}
