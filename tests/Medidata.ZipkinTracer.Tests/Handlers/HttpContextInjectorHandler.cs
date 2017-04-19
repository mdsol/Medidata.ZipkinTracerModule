using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.WebApi;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Tests
{
    public class HttpContextInjectorHandler : DelegatingHandler
    {
        public HttpContextInjectorHandler(HttpMessageHandler innerHandler): base(innerHandler) { }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpRequest = MockRepository.GenerateMock<HttpRequestBase>();
            httpRequest.Stub(x => x.Url).Return(request.RequestUri);
            httpRequest.Stub(x => x.HttpMethod).Return(request.Method.Method);
            httpRequest.Stub(x => x.Headers).Return(new NameValueCollection()
            {
                { TraceProvider.TraceIdHeaderName, "1" },
                { TraceProvider.SampledHeaderName, "1" }
            });

            var httpContext = MockRepository.GenerateMock<HttpContextBase>();
            httpContext.Stub(x => x.Request).Return(httpRequest);

            HttpContextContainer.Current = httpContext;

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
