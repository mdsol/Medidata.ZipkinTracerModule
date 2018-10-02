using System.Collections.Specialized;
using zipkin4net;

namespace Medidata.ZipkinTracer.HttpModule
{
    public interface ITraceHelper
    {
        Trace CreateTrace(NameValueCollection headers);
        ServerTrace CreateServerTrace(string serviceName, string method);
        void RecordTag(Trace trace, string key, string value);
    }
}