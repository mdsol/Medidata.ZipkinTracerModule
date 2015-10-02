using System;

namespace Medidata.ZipkinTracer.Core
{
    public interface ITracerClient
    {
        void StartServerTrace();
        void StartClientTrace(Uri clientService);
        void EndServerTrace();
        void EndClientTrace(Uri clientService);
    }
}
