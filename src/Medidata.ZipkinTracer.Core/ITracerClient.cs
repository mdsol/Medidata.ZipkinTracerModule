using System;

namespace Medidata.ZipkinTracer.Core
{
    public interface ITracerClient
    {
        void StartServerTrace();
        void StartClientTrace(string clientServiceName);
        void EndServerTrace(int duration);
        void EndClientTrace(int duration, string clientServiceName);
        void ShutDown();
    }
}
