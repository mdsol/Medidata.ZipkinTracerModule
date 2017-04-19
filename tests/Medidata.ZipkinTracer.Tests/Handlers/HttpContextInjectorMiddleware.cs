using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Owin;
using Microsoft.Owin;
using Owin;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Tests
{
    public class HttpContextInjectorMiddleware: OwinMiddleware
    {
        public HttpContextInjectorMiddleware(OwinMiddleware next): base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            httpRequest.Stub(x => x.Url).Return(context.Request.Uri);
            httpRequest.Stub(x => x.HttpMethod).Return(context.Request.Method);
            httpRequest.Stub(x => x.Headers).Return(new NameValueCollection()
            {
                { TraceProvider.TraceIdHeaderName, "1" },
                { TraceProvider.SampledHeaderName, "1" }
            });

            var httpContext = MockRepository.GenerateMock<HttpContextBase>();
            httpContext.Stub(x => x.Request).Return(httpRequest);

            if (!context.Environment.ContainsKey(typeof(HttpContextBase).FullName))
                context.Environment.Add(typeof(HttpContextBase).FullName, httpContext);

            await Next.Invoke(context);
        }
    }

    public static class HttpContextInjectorMiddlewareAppBuilderExtensions
    {
        public static void UseHttpContext(this IAppBuilder app)
        {
            app.Use<HttpContextInjectorMiddleware>();
        }
    }
}
