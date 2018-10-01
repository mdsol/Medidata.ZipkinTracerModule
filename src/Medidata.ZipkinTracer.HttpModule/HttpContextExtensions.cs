using System.Web;
using zipkin4net;
using zipkin4net.Tracers.Zipkin;
using zipkin4net.Transport.Http;

namespace Medidata.ZipkinTracer.HttpModule
{
    public static class HttpContextExtensions
    {
        public static void StartZipkinIfEnabled(this HttpContext context, IZipkinConfig config, ILogger logger = null)
        {
            if (!context.IsZipkinHttpModuleEnabled())
            {
                return;
            }

            if (logger == null)
            {
                logger = new ConsoleLogger();
            }

            TraceManager.SamplingRate = config.ZipkinSampleRate;
            TraceManager.RegisterTracer(new zipkin4net.Tracers.Zipkin.ZipkinTracer(
                new HttpZipkinSender(config.ZipkinBaseUri.AbsoluteUri, "application/json"),
                new JSONSpanSerializer()
            ));
            TraceManager.Start(logger);
        }

        public static bool IsZipkinHttpModuleEnabled(this HttpContext context)
        {
            var modules = context.ApplicationInstance.Modules;
            foreach (string moduleKey in modules.Keys)
            {
                if (modules[moduleKey] is ZipkinRequestContextModule)
                {
                    return true;
                }
            }

            return false;
        }
    }
}