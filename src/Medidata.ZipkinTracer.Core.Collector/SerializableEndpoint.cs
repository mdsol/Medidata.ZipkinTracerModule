using System;
using System.Net;
using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public class SerializableEndpoint
    {
        public SerializableEndpoint(Endpoint endpoint)
        {
            ServiceName = endpoint.Service_name;
            IpV4 = new IPAddress(BitConverter.GetBytes(endpoint.Ipv4)).ToString();
            Port = endpoint.Port;
        }

        [JsonProperty("serviceName")]
        public string ServiceName { get; private set; }

        [JsonProperty("ipv4")]
        public string IpV4 { get; private set; }

        [JsonProperty("port")]
        public int Port { get; private set; }
    }
}