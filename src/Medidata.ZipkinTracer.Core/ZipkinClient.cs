using Medidata.CrossApplicationTracer;
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

        public ZipkinClient(ITraceProvider tracerProvider, string requestName) : this(tracerProvider, requestName, new ZipkinConfig(), new SpanCollectorBuilder()) { }

        public ZipkinClient(ITraceProvider traceProvider, string requestName, IZipkinConfig zipkinConfig, ISpanCollectorBuilder spanCollectorBuilder)
        {
            isTraceOn = true;

            if ( IsConfigValuesNull(zipkinConfig) || !IsConfigValuesValid(zipkinConfig) || !IsTraceProviderValidAndSamplingOn(traceProvider))
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
                    isTraceOn = false;
                }
            }
        }

        public void StartClientTrace()
        {
            if ( isTraceOn )
            {
                clientSpan = spanTracer.SendClientSpan(requestName, traceProvider.TraceId, traceProvider.ParentSpanId, traceProvider.SpanId);
            }
        }

        public void EndClientTrace(int duration)
        {
            if ( isTraceOn )
            {
                spanTracer.ReceiveClientSpan(clientSpan, duration);
            }
        }

        public void StartServerTrace()
        {
            if ( isTraceOn )
            {
                serverSpan = spanTracer.ReceiveServerSpan(requestName, traceProvider.TraceId, traceProvider.ParentSpanId, traceProvider.SpanId);
            }
        }

        public void EndServerTrace(int duration)
        {
            if ( isTraceOn )
            {
                spanTracer.SendServerSpan(serverSpan, duration);
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
                //log("zipkinConfig.ZipkinServerName is null");
                return true;
            }

            if (String.IsNullOrEmpty(zipkinConfig.ZipkinServerPort))
            {
                //log("zipkinConfig.ZipkinServerPort is null");
                return true;
            }

            if (String.IsNullOrEmpty(zipkinConfig.ServiceName))
            {
                //log("zipkinConfig.ServiceName value is null");
                return true;
            }

            if (String.IsNullOrEmpty(zipkinConfig.SpanProcessorBatchSize))
            {
                //log("zipkinConfig.SpanProcessorBatchSize value is null");
                return true;
            }

            if (String.IsNullOrEmpty(zipkinConfig.ZipkinSampleRate))
            {
                //log("zipkinConfig.ZipkinSampleRate value is null");
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
                //log("zipkinConfig port is not an int");
                return false;
            }

            if (!int.TryParse(zipkinConfig.SpanProcessorBatchSize, out spanProcessorBatchSize))
            {
                //log("zipkinConfig spanProcessorBatchSize is not an int");
                return false;
            }
            return true;
        }

        private bool IsTraceProviderValidAndSamplingOn(ITraceProvider traceProvider)
        {
            if (traceProvider == null)
            {
                //log("traceProvider value is null");
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
