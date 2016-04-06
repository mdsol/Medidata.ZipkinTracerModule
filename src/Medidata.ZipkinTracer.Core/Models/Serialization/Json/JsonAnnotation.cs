using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Models
{
    internal class JsonAnnotation
    {
        private readonly Annotation annotation;

        [JsonProperty("endpoint")]
        public JsonEndpoint Endpoint => new JsonEndpoint(annotation.Host);

        [JsonProperty("value")]
        public string Value => annotation.Value;

        [JsonProperty("timestamp")]
        public string Timestamp => annotation.Timestamp.ToUnixTimeSeconds().ToString();

        [JsonProperty("duration")]
        public string DurationMilliseconds => annotation.DurationMilliseconds.ToString();

        public JsonAnnotation(Annotation annotation)
        {
            this.annotation = annotation;
        }
    }
}