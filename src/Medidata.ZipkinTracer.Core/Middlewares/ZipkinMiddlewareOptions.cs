namespace Medidata.ZipkinTracer.Core.Middlewares
{
    public class ZipkinMiddlewareOptions
    {
        public ZipkinMiddlewareOptions()
        {
            Enable = true;
        }

        public bool Enable { get; set; }
    }
}