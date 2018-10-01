using System.Collections;
using System.Web;

namespace Medidata.ZipkinTracer.HttpModule
{
    public interface IContextHelper
    {
        HttpRequestBase GetRequest(HttpContext c);
        HttpResponseBase GetResponse(HttpContext c);
        IDictionary GetItems(HttpContext context);
    }
}