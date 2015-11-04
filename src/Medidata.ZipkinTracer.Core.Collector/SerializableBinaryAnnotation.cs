using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public class SerializableBinaryAnnotation
    {
        public SerializableBinaryAnnotation(BinaryAnnotation binaryAnnotation)
        {
            Key = binaryAnnotation.Key;
            Value = binaryAnnotation.Value;
            SerializableEndpoint = new SerializableEndpoint(binaryAnnotation.Host);
        }

        [JsonProperty("key")]
        public string Key { get; private set; }

        [JsonProperty("value")]
        public byte[] Value { get; private set; }

        [JsonProperty("endpoint")]
        public SerializableEndpoint SerializableEndpoint { get; private set; }
    }
}
