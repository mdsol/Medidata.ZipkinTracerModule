using System.Threading.Tasks;
using Medidata.ZipkinTracer.Core;
using Microsoft.Owin;
using Owin;

namespace Medidata.ZipkinTracer.Owin
{
    internal class ZipkinMiddleware : OwinMiddleware
    {
        private readonly IZipkinConfig _config;
        private readonly SpanCollector _collector;

        public ZipkinMiddleware(OwinMiddleware next, IZipkinConfig options, SpanCollector collector = null) : base(next)
        {
            _config = options;
            _collector = collector;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var httpContext = context.ToHttpContext();

            if (_config.Bypass != null && _config.Bypass(httpContext.Request))
            {
                await Next.Invoke(context);
                return;
            }

            var zipkin = new ZipkinClient(_config, httpContext, _collector);
            var span = zipkin.StartServerTrace(context.Request.Uri, context.Request.Method);
            await Next.Invoke(context);
            zipkin.EndServerTrace(span);
        }
    }

    public static class AppBuilderExtensions
    {
        public static void UseZipkin(this IAppBuilder app, IZipkinConfig config)
        {
            config.Validate();
            app.Use<ZipkinMiddleware>(config, null);
        }

        internal static void UseZipkin(this IAppBuilder app, IZipkinConfig config, SpanCollector collector)
        {
            config.Validate();
            app.Use<ZipkinMiddleware>(config, collector);
        }
    }
}