using System.Diagnostics;
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
                    string url = HttpContext.Current.Request.Path;

                    var zipkinConfig = new ZipkinConfig();

                    var traceProvider = new TraceProvider(new System.Web.HttpContextWrapper(HttpContext.Current), zipkinConfig.DontSampleListCsv, zipkinConfig.ZipkinSampleRate);
                    var logger = LogManager.GetLogger(this.GetType());

                    ITracerClient zipkinClient = new ZipkinClient(traceProvider, url, logger);
                    zipkinClient.StartServerTrace();

                    HttpContext.Current.Items["zipkinClient"] = zipkinClient;
                };

            context.EndRequest += (sender, args) =>
                {
                    var zipkinClient = (ITracerClient)HttpContext.Current.Items["zipkinClient"];
                    zipkinClient.EndServerTrace();
                };

            context.Error += (sender, args) =>
                {
                    var zipkinClient = (ITracerClient)HttpContext.Current.Items["zipkinClient"];
                    zipkinClient.EndServerTrace();
                };
        }

        public void Dispose()
        {
            if (HttpContext.Current == null) return;
            HttpContext.Current.Items["zipkinClient"] = null;
        }
    }
}
