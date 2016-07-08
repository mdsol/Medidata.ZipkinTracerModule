using System;
using System.Runtime.CompilerServices;
using Medidata.ZipkinTracer.Models;
using Microsoft.Owin;

namespace Medidata.ZipkinTracer.Core
{
    public interface ITracerClient
    {
        bool IsTraceOn { get; }

        ITraceProvider TraceProvider { get; }

        Span StartServerTrace(IOwinRequest request);

        Span StartClientTrace(Uri remoteUri, string methodName, ITraceProvider trace);

        void EndServerTrace(Span serverSpan, IOwinContext context);

        void EndClientTrace(Span clientSpan, int statusCode);

        void Record(Span span, [CallerMemberName] string value = null);

        void RecordBinary<T>(Span span, string key, T value);

        void RecordLocalComponent(Span span, string value);
    }
}
