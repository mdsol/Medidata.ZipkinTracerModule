using System;
using System.Globalization;
using Microsoft.Owin;

namespace Medidata.ZipkinTracer.Core
{
    /// <summary>
    /// TraceProvider class
    /// </summary>
    internal class TraceProvider : ITraceProvider
    {
        public const string TraceIdHeaderName = "X-B3-TraceId";
        public const string SpanIdHeaderName = "X-B3-SpanId";
        public const string ParentSpanIdHeaderName = "X-B3-ParentSpanId";
        public const string SampledHeaderName = "X-B3-Sampled";

        /// <summary>
        /// Key name for context.Environment
        /// </summary>
        public const string Key = "Medidata.ZipkinTracer.Core.TraceProvider";

        /// <summary>
        /// Gets a TraceId
        /// </summary>
        public string TraceId { get; }

        /// <summary>
        /// Gets a SpanId
        /// </summary>
        public string SpanId { get; }

        /// <summary>
        /// Gets a ParentSpanId
        /// </summary>
        public string ParentSpanId { get; }

        /// <summary>
        /// Gets IsSampled
        /// </summary>
        public bool IsSampled { get; }

        /// <summary>
        /// Initializes a new instance of the TraceProvider class.
        /// </summary>
        /// <param name="config">ZipkinConfig instance</param>
        /// <param name="context">the IOwinContext</param>
        internal TraceProvider(IZipkinConfig config, IOwinContext context = null)
        {
            string headerTraceId = null;
            string headerSpanId = null;
            string headerParentSpanId = null;
            string headerSampled = null;

            if (context != null)
            {
                object value;
                if (context.Environment.TryGetValue(Key, out value))
                {
                    // set properties from context's item.
                    var provider = (ITraceProvider)value;
                    TraceId = provider.TraceId;
                    SpanId = provider.SpanId;
                    ParentSpanId = provider.ParentSpanId;
                    IsSampled = provider.IsSampled;
                    return;
                }

                // zipkin use the following X-Headers to propagate the trace information
                headerTraceId = GetLower16Characters(context.Request.Headers[TraceIdHeaderName]);
                headerSpanId = context.Request.Headers[SpanIdHeaderName];
                headerParentSpanId = context.Request.Headers[ParentSpanIdHeaderName];
                headerSampled = context.Request.Headers[SampledHeaderName];
            }

            TraceId = Parse(headerTraceId) ? headerTraceId : GenerateHexEncodedInt64FromNewGuid();
            SpanId = Parse(headerSpanId) ? headerSpanId : TraceId;
            ParentSpanId = Parse(headerParentSpanId) ? headerParentSpanId : string.Empty;
            IsSampled = config.ShouldBeSampled(context, headerSampled);
           
            if (SpanId == ParentSpanId)
            {
                throw new ArgumentException("x-b3-SpanId and x-b3-ParentSpanId must not be the same value.");
            }

            context?.Environment.Add(Key, this);
        }

        /// <summary>
        /// private constructor to accept property values
        /// </summary>
        /// <param name="traceId"></param>
        /// <param name="spanId"></param>
        /// <param name="parentSpanId"></param>
        /// <param name="isSampled"></param>
        internal TraceProvider(string traceId, string spanId, string parentSpanId, bool isSampled)
        {
            TraceId = traceId;
            SpanId = spanId;
            ParentSpanId = parentSpanId;
            IsSampled = isSampled;
        }

        /// <summary>
        /// Gets a Trace for outgoing HTTP request.
        /// </summary>
        /// <returns>The trace</returns>
        public ITraceProvider GetNext()
        {
            return new TraceProvider(
                TraceId,
                GenerateHexEncodedInt64FromNewGuid(),
                SpanId,
                IsSampled);
        }

        /// <summary>
        /// Parse id value
        /// </summary>
        /// <param name="value">header's value</param>
        /// <returns>true: parsed</returns>
        private bool Parse(string value)
        {
            ulong result;
            return !string.IsNullOrWhiteSpace(value) && UInt64.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Get Lower 16 Characters of an id
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetLower16Characters(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return value.Length > 16 ? value.Substring(value.Length - 16) : value;
        }

        /// <summary>
        /// Generate a hex encoded Int64 from new Guid.
        /// </summary>
        /// <returns>The hex encoded int64</returns>
        private string GenerateHexEncodedInt64FromNewGuid()
        {
            return Convert.ToString(BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0), 16);
        }
    }
}
