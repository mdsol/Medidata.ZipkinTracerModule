using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Models
{
    internal class JsonEndpoint
    {
        private readonly Endpoint endpoint;

        [JsonProperty("ipv4")]
        public string IPv4 => endpoint.IPAddress.ToString();

        [JsonProperty("port")]
        public string Port => endpoint.Port.ToString();

        [JsonProperty("serviceName")]
        public string ServiceName => endpoint.ServiceName;

        public JsonEndpoint(Endpoint endpoint)
        {
            this.endpoint = endpoint;
        }
    }
}