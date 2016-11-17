using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using Microsoft.Owin;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class ZipkinConfigTests
    {
        private ZipkinConfig _sut;

        [TestInitialize]
        public void Init()
        {
            var fixture = new Fixture();
            _sut = new ZipkinConfig
            {
                ZipkinBaseUri = new Uri("http://zipkin.com"),
                Domain = r => new Uri("http://server.com"),
                SpanProcessorBatchSize = fixture.Create<uint>(),
                ExcludedPathList = new List<string>(),
                SampleRate = 0,
                NotToBeDisplayedDomainList = new List<string>()
            };
        }

        [TestMethod]
        public void Validate()
        {
            _sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateWithNullDontSampleList()
        {
            _sut.ExcludedPathList = null;
            _sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateWithInvalirdDontSampleListItem()
        {
            _sut.ExcludedPathList = new List<string> { "xxx" };
            _sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateWithNegativeSampleRate()
        {
            _sut.SampleRate = -1;
            _sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateWithInvalidSampleRate()
        {
            _sut.SampleRate = 1.1;
            _sut.Validate();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ValidateWithNullNotToBeDisplayedDomainList()
        {
            _sut.NotToBeDisplayedDomainList = null;
            _sut.Validate();
        }

        [TestMethod]
        public void ShouldBeSampled_NullContext()
        {
            // Arrange
            var fixture = new Fixture();

            // Act
            var result = _sut.ShouldBeSampled(null, "true");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldBeSampled_NullSampledString()
        {
            // Arrange
            var fixture = new Fixture();

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString("/samplePath");
            context.Stub(x => x.Request).Return(request);

            // Act
            var result = _sut.ShouldBeSampled(context, null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldBeSampled_WhiteSpaceSampledString()
        {
            // Arrange
            var fixture = new Fixture();

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString("/samplePath");
            context.Stub(x => x.Request).Return(request);

            // Act
            var result = _sut.ShouldBeSampled(context, string.Empty);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldBeSampled_InvalidSampledString()
        {
            // Arrange
            var fixture = new Fixture();

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString("/samplePath");
            context.Stub(x => x.Request).Return(request);

            // Act
            var result = _sut.ShouldBeSampled(context, "weirdValue");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldBeSampled_PathNotInBlacktListSampleRate1_SampledStringTrue()
        {
            // Arrange
            var fixture = new Fixture();

            _sut.SampleRate = 1;

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString("/samplePath");
            context.Stub(x => x.Request).Return(request);

            // Act
            var result =_sut.ShouldBeSampled(context, "true");

            // Assert
            Assert.IsTrue(result);
        }
        
        [TestMethod]
        public void ShouldBeSampled_PathNotInBlacktListSampleRate1_SampledString1()
        {
            // Arrange
            var fixture = new Fixture();

            _sut.SampleRate = 1;

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString("/samplePath");
            context.Stub(x => x.Request).Return(request);

            // Act
            var result = _sut.ShouldBeSampled(context, "1");

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldBeSampled_PathNotInBlacktListSampleRate1_SampledStringFalse()
        {
            // Arrange
            var fixture = new Fixture();

            _sut.SampleRate = 1;

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString("/samplePath");
            context.Stub(x => x.Request).Return(request);

            // Act
            var result = _sut.ShouldBeSampled(context, "false");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldBeSampled_PathNotInBlacktListSampleRate1_SampledString0()
        {
            // Arrange
            var fixture = new Fixture();

            _sut.SampleRate = 1;

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString("/samplePath");
            context.Stub(x => x.Request).Return(request);

            // Act
            var result = _sut.ShouldBeSampled(context, "0");

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldBeSampled_PathNotInBlacktListSampleRate0()
        {
            // Arrange
            var fixture = new Fixture();

            _sut.SampleRate = 0;

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString("/samplePath");
            context.Stub(x => x.Request).Return(request);

            // Act
            var result = _sut.ShouldBeSampled(context, null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldBeSampled_PathInBlacktList()
        {
            // Arrange
            var fixture = new Fixture();

            _sut.SampleRate = 0;
            var path = "/samplePath";
            _sut.NotToBeDisplayedDomainList.Add(path);

            var context = MockRepository.GenerateStub<IOwinContext>();
            var request = MockRepository.GenerateStub<IOwinRequest>();
            request.Path = new PathString(path);
            context.Stub(x => x.Request).Return(request);

            // Act
            var result = _sut.ShouldBeSampled(context, null);

            // Assert
            Assert.IsFalse(result);
        }
    }
}