using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Specialized;
using Medidata.ZipkinTracer.Core;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class TraceProviderTests
    {
        private const string regex128BitPattern = @"^[a-f0-9]{32}$";
        private const string regex64BitPattern = @"^[a-f0-9]{16}$";

        [TestMethod]
        public void Constructor_GeneratingNew64BitTraceId()
        {
            // Arrange
            var config = new ZipkinConfig
            {
                Create128BitTraceId = false
            };

            // Arrange & Act
            var traceProvider = new TraceProvider(config);

            // Assert
            Assert.IsTrue(Regex.IsMatch(traceProvider.TraceId, regex64BitPattern));
            Assert.IsTrue(Regex.IsMatch(traceProvider.SpanId, regex64BitPattern));
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
            Assert.AreEqual(false, traceProvider.IsSampled);
        }

        [TestMethod]
        public void Constructor_GeneratingNew128BitTraceId()
        {
            // Arrange
            var config = new ZipkinConfig
            {
                Create128BitTraceId = true
            };

            // Arrange & Act
            var traceProvider = new TraceProvider(config);

            // Assert
            Assert.IsTrue(Regex.IsMatch(traceProvider.TraceId, regex128BitPattern));
            Assert.IsTrue(Regex.IsMatch(traceProvider.SpanId, regex64BitPattern));
            Assert.AreEqual(string.Empty, traceProvider.ParentSpanId);
            Assert.AreEqual(false, traceProvider.IsSampled);
        }

        [TestMethod]
        public void Constructor_HavingTraceProviderInContext()
        {
            // Arrange
            var context = MockRepository.GenerateStub<HttpContextBase>();
            var providerInContext = MockRepository.GenerateStub<ITraceProvider>();
            var environment = new Dictionary<string, object>
            {
                { "Medidata.ZipkinTracer.Core.TraceProvider", providerInContext }
            };
            context.Stub(x => x.Items).Return(environment);

            // Act
            var sut = new TraceProvider(new ZipkinConfig(), context);

            // Assert
            Assert.AreEqual(providerInContext.TraceId, sut.TraceId);
            Assert.AreEqual(providerInContext.SpanId, sut.SpanId);
            Assert.AreEqual(providerInContext.ParentSpanId, sut.ParentSpanId);
            Assert.AreEqual(providerInContext.IsSampled, sut.IsSampled);
        }

        [TestMethod]
        public void Constructor_AcceptingHeadersWith64BitTraceId()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var isSampled = fixture.Create<bool>();

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled.ToString());

            // Act
            var sut = new TraceProvider(new ZipkinConfig(), context);

            // Assert
            Assert.AreEqual(traceId, sut.TraceId);
            Assert.AreEqual(spanId, sut.SpanId);
            Assert.AreEqual(parentSpanId, sut.ParentSpanId);
            Assert.AreEqual(isSampled, sut.IsSampled);
        }

        [TestMethod]
        public void Constructor_AcceptingHeadersWithLessThan16HexCharacters()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16).Substring(1);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var isSampled = fixture.Create<bool>();

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled.ToString());

            // Act
            var sut = new TraceProvider(new ZipkinConfig(), context);

            // Assert
            Assert.AreEqual(traceId, sut.TraceId);
            Assert.AreEqual(spanId, sut.SpanId);
            Assert.AreEqual(parentSpanId, sut.ParentSpanId);
            Assert.AreEqual(isSampled, sut.IsSampled);
        }

        [TestMethod]
        public void Constructor_AcceptingHeadersWith128BitTraceId()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Guid.NewGuid().ToString("N");
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var isSampled = fixture.Create<bool>();

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled.ToString());

            // Act
            var sut = new TraceProvider(new ZipkinConfig(), context);

            // Assert
            Assert.AreEqual(traceId, sut.TraceId);
            Assert.AreEqual(spanId, sut.SpanId);
            Assert.AreEqual(parentSpanId, sut.ParentSpanId);
            Assert.AreEqual(isSampled, sut.IsSampled);
        }

        [TestMethod]
        public void Constructor_AcceptingHeadersWithOutIsSampled()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);

            var context = MockRepository.GenerateStub<HttpContextBase>();
            var request = MockRepository.GenerateStub<HttpRequestBase>();
            var headers = new NameValueCollection()
            {
                { TraceProvider.TraceIdHeaderName, traceId },
                { TraceProvider.SpanIdHeaderName, spanId },
                { TraceProvider.ParentSpanIdHeaderName,parentSpanId }
            };
            var environment = new Dictionary<string, object>();

            request.Stub(x => x.Headers).Return(headers);
            context.Stub(x => x.Request).Return(request);
            context.Stub(x => x.Items).Return(environment);

            var expectedIsSampled = fixture.Create<bool>();
            var sampleFilter = MockRepository.GenerateStub<IZipkinConfig>();
            sampleFilter.Expect(x => x.ShouldBeSampled(Arg<string>.Is.Null, Arg<string>.Is.Anything)).Return(expectedIsSampled);

            // Act
            var sut = new TraceProvider(sampleFilter, context);

            // Assert
            Assert.AreEqual(traceId, sut.TraceId);
            Assert.AreEqual(spanId, sut.SpanId);
            Assert.AreEqual(parentSpanId, sut.ParentSpanId);
            Assert.AreEqual(expectedIsSampled, sut.IsSampled);
        }

        [TestMethod]
        public void Constructor_AcceptingHeadersWithInvalidIdValues()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = fixture.Create<Guid>().ToString("N").Substring(1);
            var spanId = fixture.Create<string>();
            var parentSpanId = fixture.Create<string>(); 
            var isSampled = fixture.Create<string>();

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled);

            var expectedIsSampled = fixture.Create<bool>();
            var sampleFilter = MockRepository.GenerateStub<IZipkinConfig>();
            sampleFilter.Expect(x => x.ShouldBeSampled(Arg.Is(isSampled), Arg<string>.Is.Anything)).Return(expectedIsSampled);

            // Act
            var sut = new TraceProvider(sampleFilter, context);

            // Assert
            Assert.AreNotEqual(traceId, sut.TraceId);
            Assert.AreNotEqual(spanId, sut.SpanId);
            Assert.AreEqual(string.Empty, sut.ParentSpanId);
            Assert.AreEqual(expectedIsSampled, sut.IsSampled);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_AcceptingHeadersWithSpanAndParentSpan()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = spanId;
            var isSampled = fixture.Create<bool>();

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled.ToString());

            // Act
            new TraceProvider(new ZipkinConfig(), context);
        }

        [TestMethod]
        public void GetNext()
        {
            // Arrange
            var fixture = new Fixture();
            var traceId = Convert.ToString(fixture.Create<long>(), 16);
            var spanId = Convert.ToString(fixture.Create<long>(), 16);
            var parentSpanId = Convert.ToString(fixture.Create<long>(), 16);
            var isSampled = fixture.Create<bool>();

            var context = GenerateContext(
                traceId,
                spanId,
                parentSpanId,
                isSampled.ToString());

            var sut = new TraceProvider(new ZipkinConfig(), context);

            // Act
            var nextTraceProvider = sut.GetNext();

            // Assert
            Assert.AreEqual(sut.TraceId, nextTraceProvider.TraceId);
            Assert.IsTrue(Regex.IsMatch(nextTraceProvider.SpanId, regex64BitPattern));
            Assert.AreEqual(sut.SpanId, nextTraceProvider.ParentSpanId);
            Assert.AreEqual(sut.IsSampled, nextTraceProvider.IsSampled);
        }
        
        private HttpContextBase GenerateContext(string traceId, string spanId, string parentSpanId, string isSampled)
        {
            var context = MockRepository.GenerateStub<HttpContextBase>();
            var request = MockRepository.GenerateStub<HttpRequestBase>();
            var headers = new NameValueCollection()
            {
                { TraceProvider.TraceIdHeaderName, traceId },
                { TraceProvider.SpanIdHeaderName, spanId },
                { TraceProvider.ParentSpanIdHeaderName, parentSpanId },
                { TraceProvider.SampledHeaderName, isSampled }
            };
            var environment = new Dictionary<string, object>();

            request.Stub(x => x.Headers).Return(headers);
            context.Stub(x => x.Request).Return(request);
            context.Stub(x => x.Items).Return(environment);

            return context;
        }
    }
}
