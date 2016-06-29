using System.Threading.Tasks;
using Medidata.ZipkinTracer.Core.Logging;
using Microsoft.Owin;
using Owin;

namespace Medidata.ZipkinTracer.Core.Middlewares
{
    public class ZipkinMiddleware : OwinMiddleware
    {
        private readonly IZipkinConfig _config;

        public ZipkinMiddleware(OwinMiddleware next, IZipkinConfig options) : base(next)
        {
            _config = options;
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (_config.Bypass != null && _config.Bypass(context.Request))
            {
                await Next.Invoke(context);
                return;
            }

            var zipkin = new ZipkinClient(_config, context);
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
            app.Use<ZipkinMiddleware>(config);
        }
    }
}