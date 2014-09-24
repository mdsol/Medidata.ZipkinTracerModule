using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core
{
    public interface ITracerClient
    {
        void StartServerTrace();
        void StartClientTrace();
        void EndServerTrace(int duration);
        void EndClientTrace(int duration);
    }
}
