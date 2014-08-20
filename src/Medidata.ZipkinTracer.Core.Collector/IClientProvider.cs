using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public interface IClientProvider
    {
        void Setup();
        void Close();
        void Log(List<LogEntry> logEntries);
    }
}
