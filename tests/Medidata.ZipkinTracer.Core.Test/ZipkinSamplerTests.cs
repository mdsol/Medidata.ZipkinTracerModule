using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class ZipkinSamplerTests
    {
        private List<string> dontSampleList;

        [TestInitialize]
        public void Setup()
        {
            dontSampleList = new List<string>() { "foo", "bar"};
        }

        [TestMethod]
        public void IsInNonSampleList()
        {
            var zipkinFilter = new ZipkinSampler(dontSampleList, 0.5f);

            Assert.IsTrue(zipkinFilter.IsInDontSampleList("foo/anything"));
        }

        [TestMethod]
        public void IsInNonSampleList_NotInList()
        {
            var zipkinFilter = new ZipkinSampler(dontSampleList, 0.5f);

            Assert.IsFalse(zipkinFilter.IsInDontSampleList("notFoo/anything"));
        }

        [TestMethod]
        public void ShouldBeSampled_InNonSampleList()
        {
            var path = "foo/anything";
            var zipkinFilter = new ZipkinSampler(dontSampleList, 0.5f);

            Assert.IsFalse(zipkinFilter.ShouldBeSampled(path));
        }

        [TestMethod]
        public void ShouldBeSampled_With100PercentSampleRate()
        {
            var path = "notfoo/anything";
            var zipkinFilter = new ZipkinSampler(dontSampleList, 1.0f);

            Assert.IsTrue(zipkinFilter.ShouldBeSampled(path));
        }

        [TestMethod]
        public void ShouldBeSampled_With0PercentSampleRate()
        {
            var path = "notfoo/anything";
            var zipkinFilter = new ZipkinSampler(dontSampleList, 0.0f);

            Assert.IsFalse(zipkinFilter.ShouldBeSampled(path));
        }
    }
}
