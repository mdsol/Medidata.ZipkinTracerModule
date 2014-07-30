using Medidata.ZipkinTracerModule.Collector;
using Medidata.ZipkinTracerModule.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Medidata.ZipkinTracerModule.HttpModule
{
    public class ZipkinRequestContextModule : IHttpModule
    {
        internal IMDLogger logger;

        //public ZipkinRequestContextModule(IZipkinClient zipkinClient, IMDLogger logger)
        //{
        //    this.zipkinClient = zipkinClient;
        //    this.logger = logger;
        //}

        public void Init(HttpApplication context)
        {
            //TODO: placeholder for the "new" logger
            logger = new MDLogger();

            context.BeginRequest += (sender, args) =>
                {
                    string url = HttpContext.Current.Request.Path;

                    //TODO: move this into a separate library to get headers from HttpContext
                    var traceId = HttpContext.Current.Request.Headers["X-B3-Traceid"];
                    var parentSpanId = HttpContext.Current.Request.Headers["X-B3-Spanid"];
                    var sampled = HttpContext.Current.Request.Headers["X-B3-Sampled"];

                    var zipkinClient = new ZipkinClient();

                    Span span = StartZipkinSpan(zipkinClient, url, traceId, parentSpanId);

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

                    var zipkinClient = (IZipkinClient)HttpContext.Current.Items["zipkinClient"];
                    var span = (Span)HttpContext.Current.Items["span"];

                    EndZipkinSpan(zipkinClient, stopwatch, span);
                };
        }

        internal void EndZipkinSpan(IZipkinClient zipkinClient, Stopwatch stopwatch, Span span)
        {
            if (zipkinClient != null && span != null)
            {
                try
                {
                    zipkinClient.EndClientSpan(span, stopwatch.Elapsed.Milliseconds * 1000);
                }
                catch (Exception ex)
                {
                    logger.Event("Zipkin EndClientSpan : " + ex.Message, null, null, ex);
                }
            }
        }

        internal Span StartZipkinSpan(IZipkinClient zipkinClient, string url, string traceId, string parentSpanId)
        {
            Span span = null;

            if (traceId != null)
            {
                try
                {
                    span = zipkinClient.StartClientSpan(url, traceId, parentSpanId);
                }
                catch (Exception ex)
                {
                    logger.Event("Zipkin StartClientSpan : " + ex.Message, null,null, ex);
                }
            }
            else
            {
                var message = string.Format("Zipkin StartClientSpan : Span is not traced.\n TraceId - {0}, SpanId - {1}", traceId, parentSpanId);
                logger.Event(message, null, null);
            }

            return span;
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
