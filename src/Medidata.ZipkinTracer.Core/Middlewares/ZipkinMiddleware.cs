using System.Threading.Tasks;
using log4net;
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
            var logger = LogManager.GetLogger("ZipkinMiddleware");
            var zipkin = new ZipkinClient(logger, _config, context);
            var span = zipkin.StartServerTrace(context.Request.Uri, context.Request.Method);
            await Next.Invoke(context);
            zipkin.EndServerTrace(span);
        }
    }

    public static class AppBuilderExtensions
    {
        public static void UseZipkin(this IAppBuilder app, IZipkinConfig config)
        {
            if (!config.BypassMode)
            {
                config.Validate();
                app.Use<ZipkinMiddleware>(config);
            }
        }
    }
}