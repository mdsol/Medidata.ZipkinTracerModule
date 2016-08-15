using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;

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
    }
}