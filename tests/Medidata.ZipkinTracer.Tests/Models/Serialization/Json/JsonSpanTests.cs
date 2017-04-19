using Medidata.ZipkinTracer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using System.Linq;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class JsonSpanTests
    {
        private IFixture fixture;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
        }

        [TestMethod]
        public void JsonSpan()
        {
            // Arrange
            var span = new Span
            {
                Id = fixture.Create<string>(),
                Name = fixture.Create<string>(),
                ParentId = fixture.Create<string>(),
                TraceId = fixture.Create<string>(),
            };
            span.Annotations.Add(fixture.Create<Annotation>());
            span.Annotations.Add(fixture.Create<Annotation>());
            span.Annotations.Add(fixture.Create<BinaryAnnotation>());
            span.Annotations.Add(fixture.Create<BinaryAnnotation>());

            // Act
            var result = new JsonSpan(span);

            // Assert
            Assert.AreEqual(span.TraceId, result.TraceId);
            Assert.AreEqual(span.Name, result.Name);
            Assert.AreEqual(span.Id, result.Id);
            Assert.AreEqual(span.ParentId, result.ParentId);
            Assert.AreEqual(2, result.Annotations.Count());
            Assert.AreEqual(2, result.BinaryAnnotations.Count());
        }

        [TestMethod]
        public void JsonSpan_ParentIdIsWhiteSpace()
        {
            // Arrange
            var span = new Span
            {
                ParentId = string.Empty
            };

            // Act
            var result = new JsonSpan(span);

            // Assert
            Assert.IsNull(result.ParentId);
        }
    }
}