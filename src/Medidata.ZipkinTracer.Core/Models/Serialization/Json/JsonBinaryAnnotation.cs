using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Models
{
    internal class JsonBinaryAnnotation
    {
        private readonly BinaryAnnotation binaryAnnotation;

        [JsonProperty("endpoint")]
        public JsonEndpoint Endpoint => new JsonEndpoint(binaryAnnotation.Host);

        public string Key => binaryAnnotation.Key;

        public string Value => binaryAnnotation.Value.ToString();

        public JsonBinaryAnnotation(BinaryAnnotation binaryAnnotation)
        {
            this.binaryAnnotation = binaryAnnotation;
        }
    }
}