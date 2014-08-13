using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule
{
    public interface IZipkinClient
    {
        Span StartSpan(string requestName, string traceId, string parentSpanId, string spanId);
        void EndSpan(Span span, int duration);
    }
}
