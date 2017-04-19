using System;
using System.Web;
using Microsoft.Owin;

namespace Medidata.ZipkinTracer.Owin
{
    internal static class ZipkinTracerOwinExtensions
    {
        public static HttpContextBase ToHttpContext(this IOwinContext owinContext) =>
            owinContext.Get<HttpContextBase>(typeof(HttpContextBase).FullName) ??
            throw new NotSupportedException("Self hosted and non-System.Web-based scenarios are not supported.");
    }
}
