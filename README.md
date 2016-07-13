# Medidata.ZipkinTracerModule
A .NET implementation of the Zipkin Tracer client.

## Overview
This nuget package implements the zipkin tracer client for .net applications.

**Medidata.ZipkinTracer.Core** : core library for generating zipkin spans from ids sent through from
CrossApplicationTracer and sending it to the zipkin collector using HTTP as the transport protocol. For more information
and implementations in other languages, please check [Openzipkin](https://github.com/openzipkin/).

### Enable/Disable zipkin tracing

Zipkin will record traces if IsSampled HTTP header is true.  
This will happen if :
- **a)** the caller of the app has set the IsSampled HTTP header value to true.
- **OR**
- **b)** the url request is not in the `ExcludedPathList` of ZipkinConfig , and using the `SampleRate`, it will
determine whether or not to trace this request. `SampleRate` is the approximate percentage of traces being recorded in
zipkin.

## Configurations
Please use `ZipkinConfig` class to configure the module and verify these values and modify them according to your
service/environment.

- `Bypass` - Controls whether the requests should be sent through the Zipkin module
  - **false**: Enables the ZipkinMiddleware/ZipkinMessageHandler
  - **true**: Disables the ZipkinMiddleware/ZipkinMessageHandler
- `ZipkinBaseUri` - is the zipkin scribe/collector server URI with port to send the Spans
- `Domain` - is a valid public facing base url for your app instance. Zipkin will use to label the trace.
  - by default this looks at the incoming requests and uses the hostname from them. It's a `Func<IOwinRequest, Uri>` - customise this to your requirements.
- `SpanProcessorBatchSize` - how many Spans should be sent to the zipkin scribe/collector in one go.
- `SampleRate` - 1 decimal point float value between 0 and 1. This value will determine randomly if the current request will be traced or not.	 
- `NotToBeDisplayedDomainList`(optional) - It will be used when logging host name by excluding these strings in service name attribute
	e.g. domain: ".xyz.com", host: "abc.xyz.com" will be logged as "abc" only    
- `ExcludedPathList`(optional) - Path list that is not needed for tracing. Each item must start with "/". 


```
var config = new ZipkinConfig
{
	Bypass = request => request.Uri.AbsolutePath.StartsWith("/allowed"),
	Domain = request => new Uri("https://yourservice.com"), // or, you might like to derive a value from the request, like r => new Uri($"{r.Scheme}://{r.Host}"),
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

Note: You will need the `GetOwinContext` extension method. If you host in IIS with `System.Web`, this can be found in `Microsoft.Owin.Host.SystemWeb`.

```
using Medidata.ZipkinTracer.Core.Handlers;

public class HomeController : AsyncController
{
    public async Task<ActionResult> Index()
    {
        var context = System.Web.HttpContext.Current.GetOwinContext();
		var client = new ZipkinClient(context);

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

In case of the `ZipkinClient.Record()` method, the second parameter(`value`) can be omitted during the call, in that
case the caller member name (method, property etc.) will get recorded.

#### Recording a local component
With the `RecordLocalComponent()` method of the client a local component (or information) can be recorded for the
current trace. This will result an additional binary annotation with the 'lc' key (LOCAL_COMPONENT) and a custom value.

#### Troubleshooting

##### Logs

Logging internal to the library is provided via the [LibLog abstraction](https://github.com/damianh/LibLog). Caveat: to get logs, you must have initialised your logging framework on application-start ([console app example](https://github.com/damianh/LibLog/blob/master/src/LibLog.Example.Log4Net/Program.cs#L12) - a web-app might do this in OWIN Startup or Global.asax, or the inversion of control container initialisation).

## Contributors
ZipkinTracer is (c) Medidata Solutions Worldwide and owned by its major contributors:
* Tomoko Kwan
* [Kenya Matsumoto](https://github.com/kenyamat)
* [Brent Villanueva](https://github.com/bvillanueva-mdsol)
* [Laszlo Schreck](https://github.com/lschreck-mdsol)
