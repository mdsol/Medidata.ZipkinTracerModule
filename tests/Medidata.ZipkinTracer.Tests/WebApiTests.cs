using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Models;
using Medidata.ZipkinTracer.Owin;
using Medidata.ZipkinTracer.WebApi;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class WebApiTests
    {
        [TestMethod]
        public async Task ZipkinTraceHandler_WithBypass_WillBypassTheRequest()
        {
            var spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), (uint)1);

            var bypassUrl = new Uri("http://bypassthisurlplease");

            using (var client = new HttpClient(new HttpContextInjectorHandler(new ZipkinTraceHandler(new ZipkinConfig()
            {
                Domain = (request) => new Uri("http://localhost"),
                Bypass = (request) => request.Url == bypassUrl,
                ZipkinBaseUri = new Uri("http://whatever")
            }, new TestMessageHandler(), spanCollectorStub))))
            {
                var response = await client.GetAsync(bypassUrl);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                spanCollectorStub.AssertWasNotCalled(x => x.Start());
                spanCollectorStub.AssertWasNotCalled(x => x.Stop());
            }
        }

        [TestMethod]
        public async Task ZipkinTraceHandler_WithBypass_WillNotBypassTheRequest()
        {
            var spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), (uint)1);
            var bypassUrl = new Uri("http://bypassthisurlplease");
            var nonBypassUrl = new Uri("http://donotbypassthisurlplease");

            using (var client = new HttpClient(new HttpContextInjectorHandler(new ZipkinTraceHandler(new ZipkinConfig()
            {
                Domain = (request) => new Uri("http://localhost"),
                Bypass = (request) => request.Url == bypassUrl,
                ZipkinBaseUri = new Uri("http://whatever")
            }, new TestMessageHandler(), spanCollectorStub))))
            {
                var response = await client.GetAsync(nonBypassUrl);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                spanCollectorStub.AssertWasCalled(x => x.Start());
                spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Is.Anything));
            }
        }
    }
}
