using System.Net;

namespace Medidata.ZipkinTracer.Models
{
    public class Endpoint
    {
        public IPAddress IPAddress { get; set; }

        public ushort Port { get; set; }

        public string ServiceName { get; set; }
    }
}
