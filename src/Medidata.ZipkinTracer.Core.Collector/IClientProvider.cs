using System.Collections.Generic;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public interface IClientProvider
    {
        void Close();
        void Log(List<LogEntry> logEntries);
    }
}
