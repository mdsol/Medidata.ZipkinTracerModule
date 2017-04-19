using System;
using System.Collections.Generic;
using Medidata.ZipkinTracer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;

namespace Medidata.ZipkinTracer.Tests
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

        /// <summary>
        /// TODO: Use XUnit to do easier unit test for inline data
        /// </summary>
        private class ShouldBeSampledCondition
        {
            public string SampledFlag { get; set; }
            public string RequestPath { get; set; }
            public double SampleRate { get; set; }
            public List<string> ExcludedPathList { get; set; }
            public bool ExpectedOutcome { get; set; }

            public ShouldBeSampledCondition(
                string sampledFlag,
                string requestPath,
                double sampleRate,
                List<string> excludedPathList,                
                bool expectedOutcome)
            {
                SampledFlag = sampledFlag;
                RequestPath = requestPath;
                SampleRate = sampleRate;
                ExcludedPathList = excludedPathList;
                ExpectedOutcome = expectedOutcome;
            }
        }

        [TestMethod]
        public void ShouldBeSampled()
        {
            // Arrange
            List<ShouldBeSampledCondition> testScenarios = new List<ShouldBeSampledCondition>() {
                // sampledFlag has a valid bool string value
                { new ShouldBeSampledCondition("0", null, 0, new List<string>(), false) },
                { new ShouldBeSampledCondition("1", null, 0, new List<string>(), true) },
                { new ShouldBeSampledCondition("false", null, 0, new List<string>(), false) },
                { new ShouldBeSampledCondition("true", null, 0, new List<string>(), true) },
                { new ShouldBeSampledCondition("FALSE", null, 0, new List<string>(), false) },
                { new ShouldBeSampledCondition("TRUE", null, 0, new List<string>(), true) },
                { new ShouldBeSampledCondition("FalSe", null, 0, new List<string>(), false) },
                { new ShouldBeSampledCondition("TrUe", null, 0, new List<string>(), true) },
                // sampledFlag has an invalid bool string value and requestPath is IsInDontSampleList
                { new ShouldBeSampledCondition(null, "/x", 0, new List<string> { "/x" }, false) },
                { new ShouldBeSampledCondition("", "/x", 0, new List<string> { "/x" }, false) },
                { new ShouldBeSampledCondition("invalidValue", "/x", 0, new List<string>() { "/x" }, false) },
                // sampledFlag has an invalid bool string value, requestPath not in IsInDontSampleList, and sample rate is 0
                { new ShouldBeSampledCondition(null, null, 0, new List<string>(), false) },
                { new ShouldBeSampledCondition(null, "/x", 0, new List<string>(), false) },
                // sampledFlag has an invalid bool string value, requestPath not in IsInDontSampleList, and sample rate is 1
                { new ShouldBeSampledCondition(null, null, 1, new List<string>(), true) },
            };
            
            foreach (var testScenario in testScenarios)
            {
                var fixture = new Fixture();
                _sut = new ZipkinConfig
                {
                    ExcludedPathList = testScenario.ExcludedPathList,
                    SampleRate = testScenario.SampleRate
                };

                // Act
                var result = _sut.ShouldBeSampled(testScenario.SampledFlag, testScenario.RequestPath);

                // Assert
                Assert.AreEqual(
                    testScenario.ExpectedOutcome,
                    result,
                    "Scenario: " +
                    $"SampledFlag({testScenario.SampledFlag ?? "null"}), " +
                    $"RequestPath({testScenario.RequestPath ?? "null"}), " +
                    $"SampleRate({testScenario.SampleRate}), " +
                    $"ExcludedPathList({string.Join(",", testScenario.ExcludedPathList)}),");
            }
        }
    }
}