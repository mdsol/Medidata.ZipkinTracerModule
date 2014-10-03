using Medidata.CrossApplicationTracer;
using Medidata.MDLogging;
using Medidata.ZipkinTracer.Core.Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinClient : ITracerClient
    {
        internal readonly bool isTraceOn;
        internal SpanCollector spanCollector;
        internal SpanTracer spanTracer;
        internal Span clientSpan;
        internal Span serverSpan;

        private string requestName;
        private ITraceProvider traceProvider;
        private IMDLogger logger;

        public ZipkinClient(ITraceProvider tracerProvider, string requestName, IMDLogger logger) : this(tracerProvider, requestName, logger, new ZipkinConfig(), new SpanCollectorBuilder()) { }

        public ZipkinClient(ITraceProvider traceProvider, string requestName, IMDLogger logger, IZipkinConfig zipkinConfig, ISpanCollectorBuilder spanCollectorBuilder)
        {
            this.logger = logger;
            isTraceOn = true;

            if ( logger == null || IsConfigValuesNull(zipkinConfig) || !IsConfigValuesValid(zipkinConfig) || !IsTraceProviderValidAndSamplingOn(traceProvider))
            {
                isTraceOn = false;
            }

            if (isTraceOn)
            {
                try
                {
                    spanCollector = spanCollectorBuilder.Build(zipkinConfig.ZipkinServerName, int.Parse(zipkinConfig.ZipkinServerPort), int.Parse(zipkinConfig.SpanProcessorBatchSize));
                    spanCollector.Start();

                    spanTracer = new SpanTracer(spanCollector, zipkinConfig.ServiceName, new ServiceEndpoint());

                    this.requestName = requestName;
                    this.traceProvider = traceProvider;
                }
                catch (Exception ex)
                {
                    logger.Error("Error Building Zipkin Client Provider", ex);
                    isTraceOn = false;
                }
            }
        }

        public void StartClientTrace()
        {
            clientSpan = StartTrace(spanTracer.SendClientSpan, requestName, traceProvider.TraceId, traceProvider.ParentSpanId, traceProvider.SpanId);
        }

        public void EndClientTrace(int duration)
        {
            EndTrace(spanTracer.ReceiveClientSpan, clientSpan, duration);
        }

        public void StartServerTrace()
        {
            if (isTraceOn)
            {
                serverSpan = StartTrace(spanTracer.ReceiveServerSpan, requestName, traceProvider.TraceId, traceProvider.ParentSpanId, traceProvider.SpanId);
            }
        }

        public void EndServerTrace(int duration)
        {
            if (isTraceOn)
            {
                EndTrace(spanTracer.SendServerSpan, serverSpan, duration);
            }
        }

        private Span StartTrace(Func<string, string, string, string, Span> func, string requestName, string traceId, string parentSpanId, string spanId)
        {
            try
            {
                return func(requestName, traceId, parentSpanId, spanId);
            }
            catch (Exception ex)
            {
                logger.Error("Error Starting Trace", ex);
            }
            return null;
        }

        private void EndTrace(Action<Span, int> action, Span span, int duration)
        {
            try
            {
                action(span, duration);
            }
            catch (Exception ex)
            {
                logger.Error("Error Ending Trace", ex);
            }
        }

        public void ShutDown()
        {
            if (spanCollector != null)
            {
                spanCollector.Stop();
            }
        }

        private bool IsConfigValuesNull(IZipkinConfig zipkinConfig)
        {
            if (String.IsNullOrEmpty(zipkinConfig.ZipkinServerName))
            {
                logger.Error("zipkinConfig.ZipkinServerName is null");
                return true;
            }

            if (String.IsNullOrEmpty(zipkinConfig.ZipkinServerPort))
            {
                logger.Error("zipkinConfig.ZipkinServerPort is null");
                return true;
            }

            if (String.IsNullOrEmpty(zipkinConfig.ServiceName))
            {
                logger.Error("zipkinConfig.ServiceName value is null");
                return true;
            }

            if (String.IsNullOrEmpty(zipkinConfig.SpanProcessorBatchSize))
            {
                logger.Error("zipkinConfig.SpanProcessorBatchSize value is null");
                return true;
            }

            if (String.IsNullOrEmpty(zipkinConfig.ZipkinSampleRate))
            {
                logger.Error("zipkinConfig.ZipkinSampleRate value is null");
                return true;
            }
            return false;
        }

        private bool IsConfigValuesValid(IZipkinConfig zipkinConfig)
        {
            int port;
            int spanProcessorBatchSize;
            if (!int.TryParse(zipkinConfig.ZipkinServerPort, out port))
            {
                logger.Error("zipkinConfig port is not an int");
                return false;
            }

            if (!int.TryParse(zipkinConfig.SpanProcessorBatchSize, out spanProcessorBatchSize))
            {
                logger.Error("zipkinConfig spanProcessorBatchSize is not an int");
                return false;
            }
            return true;
        }

        private bool IsTraceProviderValidAndSamplingOn(ITraceProvider traceProvider)
        {
            if (traceProvider == null)
            {
                logger.Error("traceProvider value is null");
                return false;
            }
            else if (string.IsNullOrEmpty(traceProvider.TraceId) || !traceProvider.IsSampled)
            {
                return false;
            }
            return true;
        }

    }
}
