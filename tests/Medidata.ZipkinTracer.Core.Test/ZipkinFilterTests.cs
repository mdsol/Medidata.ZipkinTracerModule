using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class ZipkinFilterTests
    {
        private List<string> filterList;

        [TestInitialize]
        public void Setup()
        {
            filterList = new List<string>() { "foo", "bar"};
        }

        [TestMethod]
        public void IsInNonSampleList()
        {
            var zipkinFilter = new ZipkinFilter(filterList, 0.5f);

            Assert.IsTrue(zipkinFilter.IsInNonSampleList("foo/anything"));
        }

        [TestMethod]
        public void IsInNonSampleList_NotInList()
        {
            var zipkinFilter = new ZipkinFilter(filterList, 0.5f);

            Assert.IsFalse(zipkinFilter.IsInNonSampleList("notFoo/anything"));
        }

        [TestMethod]
        public void ShouldBeSampled_InNonSampleList()
        {
            var path = "foo/anything";
            var zipkinFilter = new ZipkinFilter(filterList, 0.5f);

            Assert.IsFalse(zipkinFilter.ShouldBeSampled(path));
        }

        [TestMethod]
        public void ShouldBeSampled_With100PercentSampleRate()
        {
            var path = "notfoo/anything";
            var zipkinFilter = new ZipkinFilter(filterList, 1.0f);

            Assert.IsTrue(zipkinFilter.ShouldBeSampled(path));
        }

        [TestMethod]
        public void ShouldBeSampled_With0PercentSampleRate()
        {
            var path = "notfoo/anything";
            var zipkinFilter = new ZipkinFilter(filterList, 0.0f);

            Assert.IsFalse(zipkinFilter.ShouldBeSampled(path));
        }
    }
}
