using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace Medidata.ZipkinTracer.Core.Collector
{
    [JsonObject]
    public class SerializableSpan
    {
        public SerializableSpan(Span span)
        {
            TraceId = span.Trace_id.ToString("x4");
            Name = span.Name;
            Id = span.Id.ToString("x4");
            Annotations = span.Annotations == null ? null : span.Annotations.ConvertAll(t => new SerializableAnnotation(t));
            ParentId = span.__isset.parent_id ? span.Parent_id.ToString("x4") : null;
            BinaryAnnotations = span.Binary_annotations == null ? null : span.Binary_annotations.ConvertAll(t => new SerializableBinaryAnnotation(t));
        }

        [JsonProperty("traceId")]
        public string TraceId { get; private set; }

        [JsonProperty("name")]        
        public string Name { get; private set; }

        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("parentId",DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ParentId { get; private set; }

        [JsonProperty("annotations")]
        public List<SerializableAnnotation> Annotations { get; private set; }

        [JsonProperty("binaryAnnotations")]
        public List<SerializableBinaryAnnotation> BinaryAnnotations { get; private set; }
    }
}
