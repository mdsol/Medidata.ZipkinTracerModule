using System;
using System.Runtime.CompilerServices;
using log4net;
using Medidata.ZipkinTracer.Core.Collector;
using Microsoft.Owin;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinClient: ITracerClient
    {
        internal SpanCollector spanCollector;
        internal SpanTracer spanTracer;

        private ILog logger;
        public bool IsTraceOn { get; set; }
        public ITraceProvider TraceProvider { get; }
        public IZipkinConfig ZipkinConfig { get; }

        public ZipkinClient(ILog logger, IZipkinConfig zipkinConfig, IOwinContext context = null)
            : this (logger, zipkinConfig, new SpanCollectorBuilder(), context)
        {
        }

        public ZipkinClient(ILog logger, IZipkinConfig zipkinConfig, ISpanCollectorBuilder spanCollectorBuilder, IOwinContext context = null)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (zipkinConfig == null) throw new ArgumentNullException(nameof(zipkinConfig));
            if (spanCollectorBuilder == null) throw new ArgumentNullException(nameof(spanCollectorBuilder));

            var traceProvider = new TraceProvider(zipkinConfig, context);
            IsTraceOn = zipkinConfig.Enable && IsTraceProviderSamplingOn(traceProvider);

            if (!IsTraceOn)
                return;

            zipkinConfig.Validate();
            ZipkinConfig = zipkinConfig;
            this.logger = logger;

            try
            {
                spanCollector = spanCollectorBuilder.Build(
                    zipkinConfig.ZipkinBaseUri,
                    zipkinConfig.SpanProcessorBatchSize,
                    logger);

                spanTracer = new SpanTracer(
                    spanCollector,
                    new ServiceEndpoint(),
                    zipkinConfig.NotToBeDisplayedDomainList,
                    zipkinConfig.Domain,
                    zipkinConfig.ServiceName);

                TraceProvider = traceProvider;
            }
            catch (Exception ex)
            {
                logger.Error("Error Building Zipkin Client Provider", ex);
                IsTraceOn = false;
            }
        }

        public Span StartClientTrace(Uri remoteUri, string methodName, ITraceProvider trace)
        {
            if (!IsTraceOn)
                return null;

            try
            {
                return spanTracer.SendClientSpan(
                    methodName.ToLower(),
                    trace.TraceId,
                    trace.ParentSpanId,
                    trace.SpanId,
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
            if (!IsTraceOn)
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
            if (!IsTraceOn)
                return null;

            try
            {
                return spanTracer.ReceiveServerSpan(
                    methodName.ToLower(),
                    TraceProvider.TraceId,
                    TraceProvider.ParentSpanId,
                    TraceProvider.SpanId,
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
            if (!IsTraceOn)
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
            if (!IsTraceOn)
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
            if (!IsTraceOn)
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

        /// <summary>
        /// Records a local component annotation in the span.
        /// </summary>
        /// <param name="span">The span where the annotation will be recorded.</param>
        /// <param name="value">The value of the local trace to be recorder.</param>
        public void RecordLocalComponent(Span span, string value)
        {
            if (!IsTraceOn)
                return;

            try
            {
                spanTracer.RecordBinary(span, zipkinCoreConstants.LOCAL_COMPONENT, value);
            }
            catch (Exception ex)
            {
                logger.Error($"Error recording local trace (value: {value})", ex);
            }
        }

        public void ShutDown()
        {
            if (spanCollector != null)
            {
                spanCollector.Stop();
            }
        }

        private bool IsTraceProviderSamplingOn(ITraceProvider traceProvider)
        {
            return !string.IsNullOrEmpty(traceProvider.TraceId) && traceProvider.IsSampled;
        }
    }
}
