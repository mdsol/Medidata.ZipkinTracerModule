using Medidata.ZipkinTracerModule.Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracerModule
{
    public class ZipkinClient
    {
        internal SpanCollector spanCollector;
        internal SpanTracer spanTracer;

        public ZipkinClient(IZipkinConfig zipkinConfig, ISpanCollectorBuilder spanCollectorBuilder)
        {
            if ( String.IsNullOrEmpty(zipkinConfig.ZipkinServerName)
                || String.IsNullOrEmpty(zipkinConfig.ZipkinServerPort)
                || String.IsNullOrEmpty(zipkinConfig.ServiceName))
            {
                throw new ArgumentNullException("zipkinConfig value is null");
            }
            
            int port;
            if ( !int.TryParse(zipkinConfig.ZipkinServerPort, out port) )
            {
                throw new ArgumentException("zipkinConfig port is not an int");
            }

            spanCollector = spanCollectorBuilder.Build(zipkinConfig.ZipkinServerName, port);
            spanTracer = new SpanTracer(spanCollector, zipkinConfig.ServiceName);
        }

        public void Init()
        {
            spanCollector.Start();
        }

        public void ShutDown()
        {
            spanCollector.Stop();
        }
        
        public Span StartClientSpan(string requestName, string traceId, string parentSpanId)
        {
            return spanTracer.StartClientSpan(requestName, traceId, parentSpanId);
        }

        public void EndClientSpan(Span span, int duration)
        {
            spanTracer.EndClientSpan(span, duration);
        }
    }
}
