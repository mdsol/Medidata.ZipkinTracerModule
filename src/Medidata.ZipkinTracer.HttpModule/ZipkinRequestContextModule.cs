using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Medidata.CrossApplicationTracer;
using Medidata.ZipkinTracer.HttpModule.Logging;
using Medidata.ZipkinTracer.Core;
using System.Diagnostics.CodeAnalysis;

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

                    ITracerClient zipkinClient = new ZipkinClient(traceProvider, url);

                    zipkinClient.StartServerTrace();

                    HttpContext.Current.Items["zipkinClient"] = zipkinClient;

                    var stopwatch = new Stopwatch();
                    HttpContext.Current.Items["stopwatch"] = stopwatch;
                    stopwatch.Start();
                };

            context.EndRequest += (sender, args) =>
                {
                    var stopwatch = (Stopwatch)HttpContext.Current.Items["stopwatch"];
                    stopwatch.Stop();

                    var zipkinClient = (ITracerClient)HttpContext.Current.Items["zipkinClient"];

                    zipkinClient.EndServerTrace(stopwatch.Elapsed.Milliseconds * 1000);
                };
        }

        public void Dispose()
        {
            if (HttpContext.Current == null) return;
            HttpContext.Current.Items["stopwatch"] = null;
            HttpContext.Current.Items["zipkinClient"] = null;
        }
    }
}
