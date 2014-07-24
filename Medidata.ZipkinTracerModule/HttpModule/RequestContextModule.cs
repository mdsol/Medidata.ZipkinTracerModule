using Medidata.ZipkinTracerModule.Collector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Medidata.ZipkinTracerModule.HttpModule
{
    public class RequestContextModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
//            ILog log = LogManager.GetLogger("Zipkin");

            context.BeginRequest += (sender, args) =>
                {
                    string url = HttpContext.Current.Request.Path;

                    //TODO: move this into a separate library to configure headers
                    var traceId = HttpContext.Current.Request.Headers["X-B3-Traceid"];
                    var parentSpanId = HttpContext.Current.Request.Headers["X-B3-Spanid"];
                    var sampled = HttpContext.Current.Request.Headers["X-B3-Sampled"];

                    ZipkinClient zipkinClient = null;
                    Span span = null;

                    if (Convert.ToBoolean(sampled) && traceId != null)
                    {
                        try
                        {
                            zipkinClient = new ZipkinClient(new ZipkinConfig(), new SpanCollectorBuilder());
                            span = zipkinClient.StartClientSpan(url, traceId, parentSpanId);
                        }
                        catch (Exception ex)
                        {
                            // log.Error("Zipkin StartClientSpan : " + ex.Message, ex);
                        }
                    }
                    else
                    {
                       // log.DebugFormat("Zipkin StartClientSpan : Span is not traced.\n TraceId - {0}, SpanId - {1}, Sampled - {2}", traceId, parentSpanId, sampled);
                    }

                    HttpContext.Current.Items["zipkinClient"] = zipkinClient;
                    HttpContext.Current.Items["span"] = span;

                    var stopwatch = new Stopwatch();
                    HttpContext.Current.Items["stopwatch"] = stopwatch;
                    stopwatch.Start();
                };

            context.EndRequest += (sender, args) =>
                {
                    var stopwatch = (Stopwatch)HttpContext.Current.Items["stopwatch"];
                    stopwatch.Stop();

                    var span = (Span)HttpContext.Current.Items["span"];

                    if (span != null)
                    {
                        try
                        {
                            var zipkinClient = (ZipkinClient) HttpContext.Current.Items["zipkinClient"];
                            zipkinClient.EndClientSpan(span, stopwatch.Elapsed.Milliseconds * 1000);
                        }
                        catch(Exception ex)
                        {
                            //log.Error("Zipkin EndClientSpan : " + ex.Message, ex);
                        }
                    }
                };
        }

        public void Dispose()
        {

            if (HttpContext.Current == null) return;
            HttpContext.Current.Items["stopwatch"] = null;
            HttpContext.Current.Items["span"] = null;
            HttpContext.Current.Items["zipkinClient"] = null;
        }
    }
}
