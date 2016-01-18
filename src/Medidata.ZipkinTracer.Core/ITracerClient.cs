using System;
using System.Runtime.CompilerServices;

namespace Medidata.ZipkinTracer.Core
{
    public interface ITracerClient
    {
        Span StartServerTrace(Uri requestUri, string methodName);
        Span StartClientTrace(Uri remoteUri, string methodName);
        void EndServerTrace(Span serverSpan);
        void EndClientTrace(Span clientSpan, int statusCode);
        void Record(Span span, [CallerMemberName] string value = null);
        void RecordBinary<T>(Span span, string key, T value);
        void RecordLocalComponent(Span span, string value);
    }
}
