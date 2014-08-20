using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule
{
    public interface IZipkinClient
    {
        Span StartServerSpan(string requestName, string traceId, string parentSpanId, string spanId);
        void EndServerSpan(Span span, int duration);
    }
}
