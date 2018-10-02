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

        public IDictionary GetItems(HttpContext context)
        {
            return context.Items;
        }
    }
}