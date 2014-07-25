using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule.Logging
{
    public interface IMDLogger
    {
        string Component { get; }
        string Tenant { get; set; }
        string ComponentVersion { get; set; }
        string Machine { get; }
        string Context { get; set; }
        Guid TraceId { get; }
        Guid SpanId { get; }
        Guid? ParentSpanId { get; }

        // Constructor
        // MDLogger(string component, Guid traceId, Guid spanId, string machine);

        void Event(string message, HashSet<string> tags, object data);
        void Event(string message, HashSet<string> tags, object data, Exception e);

        string EventRow(string message, HashSet<string> tags, object data, Exception e);
    }
}
