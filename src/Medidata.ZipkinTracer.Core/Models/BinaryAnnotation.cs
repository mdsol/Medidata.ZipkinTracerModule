namespace Medidata.ZipkinTracer.Models
{
    public class BinaryAnnotation: AnnotationBase
    {
        public string Key { get; set; }

        public object Value { get; set; }

        public AnnotationType AnnotationType => Value.GetType().AsAnnotationType();
    }
}
