using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Models;
using Medidata.ZipkinTracer.Owin;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Tests
{
    [TestClass]
    public class OwinTests
    {
        [TestMethod]
        public async Task ZipkinMiddleware_WithBypass_WillBypassTheRequest()
        {
            var spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), (uint)1);

            var bypassUrl = new Uri("http://bypassthisurlplease");

            using (var server = TestServer.Create(app =>
            {
                app.UseHttpContext();
                app.UseZipkin(new ZipkinConfig()
                {
                    Domain = (request) => new Uri("http://localhost"),
                    Bypass = (request) => request.Url == bypassUrl,
                    ZipkinBaseUri = new Uri("http://whatever")
                }, spanCollectorStub);

                app.Run(async context => await context.Response.WriteAsync("Done."));
            }))
            {
                var response = await server.HttpClient.GetAsync(bypassUrl);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                spanCollectorStub.AssertWasNotCalled(x => x.Start());
                spanCollectorStub.AssertWasNotCalled(x => x.Stop());
            }
        }

        [TestMethod]
        public async Task ZipkinMiddleware_WithBypass_WillNotBypassTheRequest()
        {
            var spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), (uint)1);
            var bypassUrl = new Uri("http://bypassthisurlplease");
            var nonBypassUrl = new Uri("http://donotbypassthisurlplease");

            using (var server = TestServer.Create(app =>
            {
                app.UseHttpContext();
                app.UseZipkin(new ZipkinConfig()
                {
                    Domain = (request) => new Uri("http://localhost"),
                    Bypass = (request) => request.Url == bypassUrl,
                    ZipkinBaseUri = new Uri("http://whatever")
                }, spanCollectorStub);

                app.Run(async context => await context.Response.WriteAsync("Done."));
            }))
            {
                var response = await server.HttpClient.GetAsync(nonBypassUrl);

                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                spanCollectorStub.AssertWasCalled(x => x.Start());
                spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Is.Anything));
            }
        }
    }
}
