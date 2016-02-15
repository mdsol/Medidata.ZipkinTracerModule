using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Specialized;
using Medidata.ZipkinTracer.Core;

namespace Medidata.CrossApplicationTracer.Tests
{
    /*
    [TestClass]
    public class ZipkinSamplerTests
    {
        private string dontSampleList;
        private string sampleRate;

        [TestInitialize]
        public void Setup()
        {
            dontSampleList = "foo,bar";
            sampleRate = "0.5";
        }

        [TestMethod]
        public void CTOR_WithNullSampleRateAndDontSampleList()
        {
            var zipkinSampler = new ZipkinSampler(null, null);

            Assert.AreEqual(0.0f, zipkinSampler.sampleRate);
            Assert.AreEqual(0, zipkinSampler.dontSampleList.Count);
        }

        [TestMethod]
        public void CTOR_WithNonFloatZipkinSampleRate()
        {
            var sampleRate = "asfsaf";

            var zipkinSampler = new ZipkinSampler(string.Empty, sampleRate);

            Assert.AreEqual(0.0f, zipkinSampler.sampleRate);
            Assert.AreEqual(0, zipkinSampler.dontSampleList.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CTOR_WithZipkinSampleRateLessThan0()
        {
            var sampleRate = "-0.5";

            var zipkinSampler = new ZipkinSampler(string.Empty, sampleRate);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CTOR_WithZipkinSampleRateGreaterThan1()
        {
            var sampleRate = "1.1";

            var zipkinSampler = new ZipkinSampler(string.Empty, sampleRate);
        }

        [TestMethod]
        public void IsInNonSampleList()
        {
            var zipkinFilter = new ZipkinSampler(dontSampleList, sampleRate);

            Assert.IsTrue(zipkinFilter.IsInDontSampleList("foo/anything"));
        }

        [TestMethod]
        public void IsInNonSampleList_NotInList()
        {
            var zipkinFilter = new ZipkinSampler(dontSampleList, sampleRate);

            Assert.IsFalse(zipkinFilter.IsInDontSampleList("notFoo/anything"));
        }

        [TestMethod]
        public void ShouldBeSampled_sampledTrue()
        {
            var zipkinFilter = new ZipkinSampler(dontSampleList, sampleRate);

            Assert.IsTrue(zipkinFilter.ShouldBeSampled(new StubHttpContextBase(), "true"));
        }

        [TestMethod]
        public void ShouldBeSampled_sampledFalse()
        {
            var zipkinFilter = new ZipkinSampler(dontSampleList, sampleRate);

            Assert.IsFalse(zipkinFilter.ShouldBeSampled(new StubHttpContextBase(), "false"));
        }

        [TestMethod]
        public void ShouldBeSampled_InNonSampleList()
        {
            var mockPath = "foo/sajfklsajflk";

            var httpRequestFake = new StubHttpRequestBase
            {
                PathGet = () => mockPath
            };

            var httpContextFake = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var zipkinFilter = new ZipkinSampler(dontSampleList, sampleRate);

            Assert.IsFalse(zipkinFilter.ShouldBeSampled(httpContextFake, null));
        }

        [TestMethod]
        public void ShouldBeSampled_With100PercentSampleRate()
        {
            var path = "notfoo/anything";

            var httpRequestFake = new StubHttpRequestBase
            {
                PathGet = () => path
            };

            var httpContextFake = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var zipkinFilter = new ZipkinSampler(dontSampleList, "1.0");

            Assert.IsTrue(zipkinFilter.ShouldBeSampled(httpContextFake, null));
        }

        [TestMethod]
        public void ShouldBeSampled_With0PercentSampleRate()
        {
            var path = "notfoo/anything";
            var httpRequestFake = new StubHttpRequestBase
            {
                PathGet = () => path
            };

            var httpContextFake = new StubHttpContextBase
            {
                HandlerGet = () => new StubIHttpHandler(),
                RequestGet = () => httpRequestFake,
                ItemsGet = () => new ListDictionary()
            };

            var zipkinFilter = new ZipkinSampler(dontSampleList, "0.0");

            Assert.IsFalse(zipkinFilter.ShouldBeSampled(httpContextFake, null));
        }
    }*/
}
