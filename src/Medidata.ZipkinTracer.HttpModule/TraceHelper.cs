using System.Collections.Specialized;
using zipkin4net;
using zipkin4net.Propagation;

namespace Medidata.ZipkinTracer.HttpModule
{
    public class TraceHelper : ITraceHelper
    {
        private IExtractor<NameValueCollection> _extractor;

        public TraceHelper(IExtractor<NameValueCollection> extractor = null)
        {
            _extractor = extractor ??
                Propagations.B3String.Extractor<NameValueCollection>((carrier, key) => carrier[key]);
        }

        public Trace CreateTrace(NameValueCollection headers)
        {
            var traceContext = _extractor.Extract(headers);
            var trace = traceContext == null ? Trace.Create() : Trace.CreateFromId(traceContext);
            Trace.Current = trace;
            return trace;
        }

        public ServerTrace CreateServerTrace(string serviceName, string method)
        {
            return new ServerTrace(serviceName, method);
        }

        public void RecordTag(Trace trace, string key, string value)
        {
            trace.Record(Annotations.Tag(key, value));
        }
    }
}