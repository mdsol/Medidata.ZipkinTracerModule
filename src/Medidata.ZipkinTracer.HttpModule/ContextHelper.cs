using System.Collections;
using System.Web;

namespace Medidata.ZipkinTracer.HttpModule
{
    public class ContextHelper : IContextHelper
    {
        public HttpRequestBase GetRequest(HttpContext context)
        {
            return new HttpRequestWrapper(context.Request);
        }

        public HttpResponseBase GetResponse(HttpContext context)
        {
            return new HttpResponseWrapper(context.Response);
        }

        public IDictionary GetItems(HttpContext context)
        {
            return context.Items;
        }
    }
}