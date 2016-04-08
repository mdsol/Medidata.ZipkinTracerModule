using System;
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
        public string Timestamp => annotation.Timestamp.ToUnixTimeMicroseconds().ToString();

        [JsonProperty("duration")]
        public string DurationMilliseconds => annotation.DurationMilliseconds.ToString();

        public JsonAnnotation(Annotation annotation)
        {
            if (annotation == null)
                throw new ArgumentNullException(nameof(annotation));

            this.annotation = annotation;
        }
    }
}