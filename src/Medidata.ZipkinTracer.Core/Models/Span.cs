using System;
using System.Collections.Generic;

namespace Medidata.ZipkinTracer.Models
{
    public class Span
    {
        public string TraceId { get; set; }

        public string Name { get; set; }

        public string Id { get; set; }

        public string ParentId { get; set; }

        public IList<AnnotationBase> Annotations { get; } = new List<AnnotationBase>();
    }
}
