using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Medidata.ZipkinTracer.Core;

namespace Medidata.ZipkinTracer.WebApi
{
    public class ZipkinTraceHandler: DelegatingHandler
    {
        private readonly IZipkinConfig config;
        private readonly SpanCollector collector;

        public ZipkinTraceHandler(IZipkinConfig config, SpanCollector collector = null) : base()
        {
            this.config = config;
            this.collector = collector;
        }

        public ZipkinTraceHandler(IZipkinConfig config, HttpMessageHandler innerHandler, SpanCollector collector = null)
            : base(innerHandler)
        {
            this.config = config;
            this.collector = collector;
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (InnerHandler == null)
                InnerHandler = new HttpClientHandler();

            var context = HttpContextContainer.Current;

            if (config.Bypass?.Invoke(context.Request) ?? false)
                return await base.SendAsync(request, cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext: false);

            var zipkin = new ZipkinClient(config, context, collector);
            var span = zipkin.StartServerTrace(context.Request.Url, context.Request.HttpMethod);

            var result = await base.SendAsync(request, cancellationToken)
                .ConfigureAwait(continueOnCapturedContext: false);

            zipkin.EndServerTrace(span);

            return result;
        }
    }
}
