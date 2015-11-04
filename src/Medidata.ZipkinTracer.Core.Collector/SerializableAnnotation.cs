using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Core.Collector
{
    [JsonObject]
    public class SerializableAnnotation
    {
        public SerializableAnnotation(Annotation annotation)
        {
            TimeStamp = annotation.Timestamp;
            Value = annotation.Value;
            SerializableEndpoint = new SerializableEndpoint(annotation.Host);
        }

        [JsonProperty("timestamp")]
        public long TimeStamp { get; private set; }
        
        [JsonProperty("value")]
        public string Value { get; private set; }
        
        [JsonProperty("endpoint")]
        public SerializableEndpoint SerializableEndpoint { get; private set; }
    }
}
