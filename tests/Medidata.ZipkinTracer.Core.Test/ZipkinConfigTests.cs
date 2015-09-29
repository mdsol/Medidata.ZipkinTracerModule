using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class ZipkinConfigTests
    {
        #region Get Internal Domain List
        [TestMethod]
        public void GetInternalDomainList()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            ConfigurationManager.AppSettings["notToBeDisplayedDomainList"] = ".xyz.net,.abc.com";

            // Act
            var result = config.GetNotToBeDisplayedDomainList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(".xyz.net", result[0]);
            Assert.AreEqual(".abc.com", result[1]);
        }

        [TestMethod]
        public void GetInternalDomainListWithEmptyConfig()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            ConfigurationManager.AppSettings["notToBeDisplayedDomainList"] = "";

            // Act
            var result = config.GetNotToBeDisplayedDomainList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetInternalDomainListWithOnlyCommaLocalesConfigValues()
        {
            // Arrange
            ZipkinConfig config = new ZipkinConfig();
            ConfigurationManager.AppSettings["notToBeDisplayedDomainList"] = ",";

            // Act
            var result = config.GetNotToBeDisplayedDomainList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
        #endregion
    }
}
