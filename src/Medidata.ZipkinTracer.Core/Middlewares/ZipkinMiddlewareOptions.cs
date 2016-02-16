namespace Medidata.ZipkinTracer.Core.Middlewares
{
    public class ZipkinMiddlewareOptions : ZipkinConfig
    {
        public bool Enable { get; set; } = true;
    }
}