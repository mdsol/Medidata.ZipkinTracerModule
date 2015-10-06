using System.Web;
using Medidata.CrossApplicationTracer;
using Medidata.ZipkinTracer.Core;
using System.Diagnostics.CodeAnalysis;
using log4net;

namespace Medidata.ZipkinTracer.HttpModule
{
    [ExcludeFromCodeCoverage]
    public class ZipkinRequestContextModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.BeginRequest += (sender, args) =>
            {
                var zipkinConfig = new ZipkinConfig();

                var traceProvider = new TraceProvider(new HttpContextWrapper(HttpContext.Current), zipkinConfig.DontSampleListCsv, zipkinConfig.ZipkinSampleRate);
                var logger = LogManager.GetLogger(GetType());

                ITracerClient zipkinClient = new ZipkinClient(traceProvider, logger);
                var span = zipkinClient.StartServerTrace(HttpContext.Current.Request.Url, HttpContext.Current.Request.HttpMethod);

                HttpContext.Current.Items["zipkinClient"] = zipkinClient;
                HttpContext.Current.Items["zipkinSpan"] = span;
            };

            context.EndRequest += (sender, args) =>
            {
                var zipkinClient = (ITracerClient)HttpContext.Current.Items["zipkinClient"];
                var span = (Span)HttpContext.Current.Items["zipkinSpan"];
                zipkinClient.EndServerTrace(span);
            };
        }

        public void Dispose()
        {
            if (HttpContext.Current == null) return;
            HttpContext.Current.Items["zipkinClient"] = null;
            HttpContext.Current.Items["zipkinSpan"] = null;
        }
    }
}
