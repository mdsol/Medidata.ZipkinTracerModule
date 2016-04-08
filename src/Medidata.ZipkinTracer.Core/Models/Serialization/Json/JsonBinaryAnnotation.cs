using System;
using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Models
{
    internal class JsonBinaryAnnotation
    {
        private readonly BinaryAnnotation binaryAnnotation;

        [JsonProperty("endpoint")]
        public JsonEndpoint Endpoint => new JsonEndpoint(binaryAnnotation.Host);

        [JsonProperty("key")]
        public string Key => binaryAnnotation.Key;

        [JsonProperty("value")]
        public string Value => binaryAnnotation.Value.ToString();

        public JsonBinaryAnnotation(BinaryAnnotation binaryAnnotation)
        {
            if (binaryAnnotation == null)
                throw new ArgumentNullException(nameof(binaryAnnotation));

            this.binaryAnnotation = binaryAnnotation;
        }
    }
}