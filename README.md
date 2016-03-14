# Medidata.ZipkinTracerModule
A .NET implementation of the Zipkin Tracer client.

## Overview
This nuget package implements the zipkin tracer client for .net applications.

**Medidata.ZipkinTracer.Core** : core library for generating zipkin spans from ids sent through from CrossApplicationTracer and sending it to the zipkin collector using thrift protocol. For more information and implementations in other languages, please check [Openzipkin](https://github.com/openzipkin/).

### Enable/Disable zipkin tracing

Zipkin will record traces if IsSampled HTTP header is true.  
This will happen if :
- **a)** the caller of the app has set the IsSampled HTTP header value to true.
- **OR**
- **b)** the url request is not in the `ExcludedPathList` of ZipkinConfig , and using the `SampleRate`, it will determine whether or not to trace this request. `SampleRate` is the approximate percentage of traces being recorded in zipkin.

## Configurations
Please use `ZipkinConig` class to configure the module and verify these values and modify them according to your service/environment.

- `Bypass` - **false**: enable ZipkinMiddleware/ZipkinMessageHandler, **true**: disable ZipkinMiddleware/ZipkinMessageHandler.
- `ZipkinBaseUri` - is the zipkin scribe/collector server URI with port to send the Spans
- `Domain` - is a valid public facing base url for your app instance. Zipkin will use to label the trace.
- `SpanProcessorBatchSize` - how many Spans should be sent to the zipkin scribe/collector in one go.
- `SampleRate` - 1 decimal point float value between 0 and 1. This value will determine randomly if the current request will be traced or not.	 
- `NotToBeDisplayedDomainList`(optional) - It will be used when logging host name by excluding these strings in service name attribute
	e.g. domain: ".xyz.com", host: "abc.xyz.com" will be logged as "abc" only    
- `ExcludedPathList`(optional) - Path list that is not needed for tracing. Each item must start with "/". 


```
var config = new ZipkinConfig
{
	Bypass = request => request.Uri.AbsolutePath.StartsWith("/allowed"),
	Domain = new Uri("https://yourservice.com"),
	ZipkinBaseUri = new Uri("http://zipkin.xyz.net:9411"),
	SpanProcessorBatchSize = 10,
	SampleRate = 0.5,
	NotToBeDisplayedDomainList = new[] { ".xyz.com", ".myApplication.net" },
	ExcludedPathList = new[] { "/check_uri", "/status" }
}
```

## Tracing

### Server trace (Inbound request)
Server Trace relies on OWIN Middleware. Please create OWIN Startup class then call `UseZipkin()`.


```
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Core.Middlewares;

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
		app.UseZipkin(new ZipkinConfig
		{
		    Domain = new Uri("https://yourservice.com"),
			ZipkinBaseUri = new Uri("http://zipkin.xyz.net:9411"),
			SpanProcessorBatchSize = 10,
		    SampleRate = 0.5    
		};
    }
}

```

### Client trace (Outbound request)
Client Trace relies on HttpMessageHandler for HttpClient. Please pass a ZipkinMessageHandler instance into HttpClient. 


```
using Medidata.ZipkinTracer.Core.Handlers;

public class HomeController : AsyncController
{
	private ILog logger = LogManager.GetLogger("HomeController");

    public async Task<ActionResult> Index()
    {
        var context = System.Web.HttpContext.Current.GetOwinContext();
		var client = new ZipkinClient(logger, context);

        using (var httpClient = new HttpClient(new ZipkinMessageHandler(client))))
        {
            var response = await httpClient.GetAsync("http://www.google.com");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
            }
        }

        return View();
    }
}
```

#### Recording arbitrary events and additional information
Additional annotations can be recorded by using the ZipkinClient's `Record()` and `RecordBinary<T>()` methods:

```
var zipkinClient = (ITracerClient)HttpContext.Current.Items["zipkinClient"];
var url = "https://abc.xyz.com:8000";
var requestUri = "/object/1";
HttpResponseMessage result;
using (var client = new HttpClient())
{
    client.BaseAddress = new Uri(url);

	// start client trace
    var span = tracerClient.StartClientTrace(new Uri(client.BaseAddress, requestUri), "GET");

    tracerClient.Record(span, "A description which will gets recorded with a timestamp.");

    result = await client.GetAsync(requestUri);

    // Record the total memory used after the call
    tracerClient.RecordBinary(span, "client.memory", GC.GetTotalMemory(false));

	// end client trace
    tracerClient.EndClientTrace(span);	
}
...
```

In case of the `ZipkinClient.Record()` method, the second parameter(`value`) can be omitted during the call, in that case the caller member name (method, property etc.) will get recorded.

#### Recording a local component
With the `RecordLocalComponent()` method of the client a local component (or information) can be recorded for the current trace. This will result an additional binary annotation with the 'lc' key (LOCAL_COMPONENT) and a custom value.

## Contributors
ZipkinTracer is (c) Medidata Solutions Worldwide and owned by its major contributors:
* Tomoko Kwan
* [Kenya Matsumoto](https://github.com/kenyamat)
* [Brent Villanueva](https://github.com/bvillanueva-mdsol)
* [Laszlo Schreck](https://github.com/lschreck-mdsol)
