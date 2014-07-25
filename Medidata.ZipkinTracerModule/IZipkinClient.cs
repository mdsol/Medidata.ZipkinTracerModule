using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule
{
    public interface IZipkinClient
    {
        Span StartClientSpan(string requestName, string traceId, string parentSpanId);
        void EndClientSpan(Span span, int duration);
    }
}
