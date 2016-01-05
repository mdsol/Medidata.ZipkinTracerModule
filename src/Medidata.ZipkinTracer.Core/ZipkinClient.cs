using Medidata.CrossApplicationTracer;
using Medidata.ZipkinTracer.Core.Collector;
using log4net;
using System;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinClient : ITracerClient
    {
        internal bool isTraceOn;
        internal SpanCollector spanCollector;
        internal SpanTracer spanTracer;

        private ITraceProvider traceProvider;
        private ILog logger;

        public ZipkinClient(ITraceProvider tracerProvider, ILog logger) : this(tracerProvider, logger, new ZipkinConfig(), new SpanCollectorBuilder()) { }

        public ZipkinClient(ITraceProvider traceProvider, ILog logger, IZipkinConfig zipkinConfig, ISpanCollectorBuilder spanCollectorBuilder)
        {
            this.logger = logger;
            isTraceOn = true;

            if ( logger == null || !IsConfigValuesValid(zipkinConfig) || !IsTraceProviderValidAndSamplingOn(traceProvider))
            {
                isTraceOn = false;
            }

            if (isTraceOn)
            {
                try
                {
                    spanCollector = spanCollectorBuilder.Build(
                        new Uri(zipkinConfig.ZipkinBaseUri),
                        int.Parse(zipkinConfig.SpanProcessorBatchSize),
                        logger);

                    spanTracer = new SpanTracer(
                        spanCollector,
                        new ServiceEndpoint(),
                        zipkinConfig.GetNotToBeDisplayedDomainList(),
                        zipkinConfig.Domain,
                        zipkinConfig.ServiceName);

                    this.traceProvider = traceProvider;
                }
                catch (Exception ex)
                {
                    logger.Error("Error Building Zipkin Client Provider", ex);
                    isTraceOn = false;
                }
            }
        }

        public Span StartClientTrace(Uri remoteUri, string methodName)
        {
            if (isTraceOn)
            {
                try
                {
                    var nextTrace = traceProvider.GetNext();
                    return spanTracer.SendClientSpan(
                        methodName.ToLower(),
                        nextTrace.TraceId,
                        nextTrace.ParentSpanId,
                        nextTrace.SpanId,
                        remoteUri);
                }
                catch (Exception ex)
                {
                    logger.Error("Error Starting Client Trace", ex);
                }
            }
            return null;
        }

        public void EndClientTrace(Span clientSpan, int statusCode)
        {
            if (isTraceOn)
            {
                try
                {
                    spanTracer.ReceiveClientSpan(clientSpan, statusCode);
                }
                catch (Exception ex)
                {
                    logger.Error("Error Ending Client Trace", ex);
                }
            }
        }

        public Span StartServerTrace(Uri requestUri, string methodName)
        {
            if (isTraceOn)
            {
                try
                {
                    return spanTracer.ReceiveServerSpan(
                        methodName.ToLower(),
                        traceProvider.TraceId,
                        traceProvider.ParentSpanId,
                        traceProvider.SpanId,
                        requestUri);
                }
                catch (Exception ex)
                {
                    logger.Error("Error Starting Server Trace", ex);
                }
            }
            return null;
        }

        public void EndServerTrace(Span serverSpan)
        {
            if (isTraceOn)
            {
                try
                {
                    spanTracer.SendServerSpan(serverSpan);
                }
                catch (Exception ex)
                {
                    logger.Error("Error Ending Server Trace", ex);
                }
            }
        }

        public void ShutDown()
        {
            if (spanCollector != null)
            {
                spanCollector.Stop();
            }
        }

        private bool IsConfigValuesValid(IZipkinConfig zipkinConfig)
        {
            if (String.IsNullOrEmpty(zipkinConfig.ZipkinBaseUri))
            {
                logger.Error("zipkinConfig.ZipkinBaseUri is null");
                return false;
            }

            if (String.IsNullOrEmpty(zipkinConfig.ServiceName))
            {
                logger.Error("zipkinConfig.ServiceName value is null");
                return false;
            }

            if (String.IsNullOrEmpty(zipkinConfig.SpanProcessorBatchSize))
            {
                logger.Error("zipkinConfig.SpanProcessorBatchSize value is null");
                return false;
            }

            if (String.IsNullOrEmpty(zipkinConfig.ZipkinSampleRate))
            {
                logger.Error("zipkinConfig.ZipkinSampleRate value is null");
                return false;
            }

            int spanProcessorBatchSize;

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

            if (string.IsNullOrEmpty(traceProvider.TraceId) || !traceProvider.IsSampled)
            {
                return false;
            }
            return true;
        }

    }
}
