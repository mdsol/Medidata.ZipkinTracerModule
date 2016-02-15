using System;

namespace Medidata.ZipkinTracer.Core.Handlers
{
    public class ZipkinBinaryAnnotation
    {
        public string Key { get; set; }
        public Func<object> Func { get; set; }
    }
}
