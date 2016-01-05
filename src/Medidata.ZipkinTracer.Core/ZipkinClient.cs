using System;
using System.Runtime.CompilerServices;
using log4net;
using Medidata.CrossApplicationTracer;
using Medidata.ZipkinTracer.Core.Collector;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinClient: ITracerClient
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

            isTraceOn = logger != null && IsConfigValuesValid(zipkinConfig) && IsTraceProviderValidAndSamplingOn(traceProvider);

            if (!isTraceOn)
                return;

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

        public Span StartClientTrace(Uri remoteUri, string methodName)
        {
            if (!isTraceOn)
                return null;

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
                return null;
            }
        }

        public void EndClientTrace(Span clientSpan, int statusCode)
        {
            if (!isTraceOn)
                return;

            try
            {
                spanTracer.ReceiveClientSpan(clientSpan, statusCode);
            }
            catch (Exception ex)
            {
                logger.Error("Error Ending Client Trace", ex);
            }
        }

        public Span StartServerTrace(Uri requestUri, string methodName)
        {
            if (!isTraceOn)
                return null;

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
                return null;
            }
        }

        public void EndServerTrace(Span serverSpan)
        {
            if (!isTraceOn)
                return;

            try
            {
                spanTracer.SendServerSpan(serverSpan);
            }
            catch (Exception ex)
            {
                logger.Error("Error Ending Server Trace", ex);
            }
        }

        /// <summary>
        /// Records an annotation with the current timestamp and the provided value in the span.
        /// </summary>
        /// <param name="span">The span where the annotation will be recorded.</param>
        /// <param name="value">The value of the annotation to be recorded. If this parameter is omitted
        /// (or its value set to null), the method caller member name will be automatically passed.</param>
        public void Record(Span span, [CallerMemberName] string value = null)
        {
            if (!isTraceOn)
                return;

            try
            {
                spanTracer.Record(span, value);
            }
            catch (Exception ex)
            {
                logger.Error("Error recording the annotation", ex);
            }
        }

        /// <summary>
        /// Records a key-value pair as a binary annotiation in the span.
        /// </summary>
        /// <typeparam name="T">The type of the value to be recorded. See remarks for the currently supported types.</typeparam>
        /// <param name="span">The span where the annotation will be recorded.</param>
        /// <param name="key">The key which is a reference to the recorded value.</param>
        /// <param name="value">The value of the annotation to be recorded.</param>
        /// <remarks>The RecordBinary will record a key-value pair which can be used to tag some additional information
        /// in the trace without any timestamps. The currently supported value types are <see cref="bool"/>,
        /// <see cref="byte[]"/>, <see cref="short"/>, <see cref="int"/>, <see cref="long"/>, <see cref="double"/> and
        /// <see cref="string"/>. Any other types will be passed as string annotation types.
        /// 
        /// Please note, that although the values have types, they will be recorded and sent by calling their
        /// respective ToString() method.</remarks>
        public void RecordBinary<T>(Span span, string key, T value)
        {
            if (!isTraceOn)
                return;

            try
            {
                spanTracer.RecordBinary<T>(span, key, value);
            }
            catch (Exception ex)
            {
                logger.Error($"Error recording a binary annotation (key: {key})", ex);
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

            return !string.IsNullOrEmpty(traceProvider.TraceId) && traceProvider.IsSampled;
        }
    }
}
