using System.Collections.Generic;

namespace Medidata.ZipkinTracer.Models
{
    public class Span
    {
        public long TraceId { get; set; }

        public string Name { get; set; }

        public long Id { get; set; }

        public long? ParentId { get; set; }

        public IList<AnnotationBase> Annotations { get; } = new List<AnnotationBase>();
    }
}
