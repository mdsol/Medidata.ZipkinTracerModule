using System;

namespace Medidata.ZipkinTracer.Core
{
    public interface ITracerClient
    {
        Span StartServerTrace(Uri requestUri, string methodName);
        Span StartClientTrace(Uri remoteUri, string methodName);
        void EndServerTrace(Span serverSpan);
        void EndClientTrace(Span clientSpan, int statusCode);
    }
}
