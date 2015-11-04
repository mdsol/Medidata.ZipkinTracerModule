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
    }
}
