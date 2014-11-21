using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Medidata.CrossApplicationTracer;
using Medidata.ZipkinTracer.Core;
using System.Diagnostics.CodeAnalysis;
using Medidata.MDLogging;
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
                    var logger = new MDLogger(LogManager.GetLogger(this.GetType()), traceProvider);

                    ITracerClient zipkinClient = new ZipkinClient(traceProvider, url, logger);

                    zipkinClient.StartServerTrace();

                    HttpContext.Current.Items["zipkinClient"] = zipkinClient;

                    var stopwatch = new Stopwatch();
                    HttpContext.Current.Items["zipkinStopwatch"] = stopwatch;
                    stopwatch.Start();
                };

            context.EndRequest += (sender, args) =>
                {
                    var stopwatch = (Stopwatch)HttpContext.Current.Items["zipkinStopwatch"];
                    stopwatch.Stop();

                    var zipkinClient = (ITracerClient)HttpContext.Current.Items["zipkinClient"];

                    zipkinClient.EndServerTrace(stopwatch.Elapsed.Milliseconds * 1000);
                };
        }

        public void Dispose()
        {
            if (HttpContext.Current == null) return;
            HttpContext.Current.Items["zipkinStopwatch"] = null;
            HttpContext.Current.Items["zipkinClient"] = null;
        }
    }
}
