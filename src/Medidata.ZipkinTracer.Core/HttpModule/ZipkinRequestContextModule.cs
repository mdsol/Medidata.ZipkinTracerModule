using Medidata.ZipkinTracer.Core.Collector;
using Medidata.ZipkinTracer.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Medidata.CrossApplicationTracer;

namespace Medidata.ZipkinTracer.Core.HttpModule
{
    public class ZipkinRequestContextModule : IHttpModule
    {
        internal IMDLogger logger;

        public void Init(HttpApplication context)
        {
            //TODO: placeholder for the "new" logger
            logger = new MDLogger();

            context.BeginRequest += (sender, args) =>
                {
                    string url = HttpContext.Current.Request.Path;

                    var traceProvider = new TraceProvider(new System.Web.HttpContextWrapper(HttpContext.Current));
                    var traceId = traceProvider.TraceId;
                    var parentSpanId = traceProvider.ParentSpanId;
                    var spanId = traceProvider.SpanId;

                    IZipkinClient zipkinClient = InitializeZipkinClient();

                    var span = StartZipkinSpan(zipkinClient, url, traceId, parentSpanId, spanId);

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

        private IZipkinClient InitializeZipkinClient()
        {
            IZipkinClient zipkinClient = null;
            try
            {
                zipkinClient = new ZipkinClient();
            }
            catch (Exception ex)
            {
                logger.Event("Initialize ZipkinClient : " + ex.Message, null, null, ex);
            }
            return zipkinClient;
        }

        internal void EndZipkinSpan(IZipkinClient zipkinClient, Stopwatch stopwatch, Span span)
        {
            if (zipkinClient != null && span != null)
            {
                try
                {
                    zipkinClient.EndServerSpan(span, stopwatch.Elapsed.Milliseconds * 1000);
                }
                catch (Exception ex)
                {
                    logger.Event("Zipkin EndClientSpan : " + ex.Message, null, null, ex);
                }
            }
        }

        internal Span StartZipkinSpan(IZipkinClient zipkinClient, string url, string traceId, string parentSpanId, string spanId)
        {
            Span span = null;

            logger.Event(String.Format("TraceId - {0}, ParentSpanId - {1}, SpanId - {2}", traceId, parentSpanId,  spanId), null, null);

            if ( string.IsNullOrEmpty(traceId))
            {
                logger.Event("traceId is null or empty", null, null);
            }
            else if (string.IsNullOrEmpty(spanId))
            {
                logger.Event("spanId is null or empty", null, null);
            }
            else
            {
                try
                {
                    span = zipkinClient.StartServerSpan(url, traceId, parentSpanId, spanId);
                }
                catch (Exception ex)
                {
                    logger.Event("Zipkin StartClientSpan : " + ex.Message, null, null, ex);
                }
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
