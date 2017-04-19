using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Medidata.ZipkinTracer.Core.Helpers;

namespace Medidata.ZipkinTracer.Tests.Helpers
{
    [TestClass]
    public class ParserHelperTests
    {
        #region IsParsableTo128Or64Bit

        [TestMethod]
        public void IsParsableTo128Or64Bit_ValidLongHexStringRepresentation()
        {
            // Arrange
            string value = long.MaxValue.ToString("x");

            // Act
            var result = ParserHelper.IsParsableTo128Or64Bit(value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsParsableTo128Or64Bit_ValidGuidStringRepresentation()
        {
            // Arrange
            string value = Guid.NewGuid().ToString("N");

            // Act
            var result = ParserHelper.IsParsableTo128Or64Bit(value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsParsableTo128Or64Bit_InvalidLongHexStringRepresentation()
        {
            // Arrange
            var longStringRepresentation = long.MaxValue.ToString("x");
            string value = longStringRepresentation.Substring(1) + "k" ;

            // Act
            var result = ParserHelper.IsParsableTo128Or64Bit(value);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region IsParsableToGuid

        [TestMethod]
        public void IsParsableToGuid_Null()
        {
            // Arrange
            string value = null;

            // Act
            var result = ParserHelper.IsParsableToGuid(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsParsableToGuid_WhiteSpace()
        {
            // Arrange
            string value = string.Empty;

            // Act
            var result = ParserHelper.IsParsableToGuid(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsParsableToGuid_MoreThan32Characters()
        {
            // Arrange
            string value = Guid.NewGuid().ToString("N") + "a";

            // Act
            var result = ParserHelper.IsParsableToGuid(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsParsableToGuid_LessThan32Characters()
        {
            // Arrange
            string value = Guid.NewGuid().ToString("N").Substring(1);

            // Act
            var result = ParserHelper.IsParsableToGuid(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsParsableToGuid_32CharactersWithInvalidCharacter()
        {
            // Arrange
            string value = Guid.NewGuid().ToString("N").Substring(1) + "x";

            // Act
            var result = ParserHelper.IsParsableToGuid(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsParsableToGuid_32Characters()
        {
            // Arrange
            string value = Guid.NewGuid().ToString("N");

            // Act
            var result = ParserHelper.IsParsableToGuid(value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsParsableToGuid_GuidWithHyphens()
        {
            // Arrange
            string value = Guid.NewGuid().ToString();

            // Act
            var result = ParserHelper.IsParsableToGuid(value);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region IsParsableToLong

        [TestMethod]
        public void IsParsableToLong_Null()
        {
            // Arrange
            string value = null;

            // Act
            var result = ParserHelper.IsParsableToLong(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsParsableToLong_WhiteSpace()
        {
            // Arrange
            string value = string.Empty;

            // Act
            var result = ParserHelper.IsParsableToLong(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsParsableToLong_MinLong()
        {
            // Arrange
            var longValue = long.MinValue;
            string value = longValue.ToString("x");

            // Act
            var result = ParserHelper.IsParsableToLong(value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsParsableToLong_MaxLong()
        {
            // Arrange
            var longValue = long.MaxValue;
            string value = longValue.ToString("x4");

            // Act
            var result = ParserHelper.IsParsableToLong(value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsParsableToLong_MoreThan16Characters()
        {
            // Arrange
            var longValue = long.MaxValue;
            string value = longValue.ToString("x") + "a";

            // Act
            var result = ParserHelper.IsParsableToLong(value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsParsableToLong_HasInvalidCharacter()
        {
            // Arrange
            var longValue = long.MaxValue;
            string value = longValue.ToString("x4").Remove(15) + "x";

            // Act
            var result = ParserHelper.IsParsableToLong(value);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}