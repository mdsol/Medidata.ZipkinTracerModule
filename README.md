# Medidata.ZipkinTracerModule
A .NET implementation of the Zipkin Tracer client.

## Overview
This nuget package implements the zipkin tracer client for .net applications.  This sln produces 2 nuget packages.

1) Medidata.ZipkinTracer.Core : core library for generating zipkin spans from ids sent through from CrossApplicationTracer and sending it to the zipkin collector using thrift protocol

2) Medidata.ZipkinTracer.HttpModule : httpModule injects zipkin trace into every request which has a zipkin traceId and isSampled of true in the header. Includes config transforms that automatically modifies the config for zipkin to work.

For more information and implementations in other languages, please check [Openzipkin](https://github.com/openzipkin/).

### Enable/Disable zipkin tracing

Zipkin relies on CrossApplicationTracer library (https://github.com/mdsol/Medidata.CrossApplicationTracer)'s TraceProvider to retrieve ids which are passed through the http request headers.

Zipkin will record traces if TraceProvider's IsSampled is true.  

This will happen if :

a) the caller of the app has set the IsSampled http header value to true.

OR

b) the url request is not in the mAuthWhitelist appsetting config, and using the zipkinSampleRate, CrossApplicationTracer will determine whether or not to trace this request. zipkinSampleRate is the approximate percentage of traces being recorded in zipkin.

### Configurations
Below are the configurations that are needed.  

1) appsettings.config

Add the below additional configurations. Please verify these values and modify them according to your service/environment.

```
<appSettings>
  <add key="domain" value="ampridatvir-sandbox.xyz.com" />
  <add key="zipkinScribeServerName" value="zipkinvm.cloudapp.net" />
  <add key="zipkinScribeServerPort" value="9410" />
  <add key="zipkinServiceName" value="Name of your Service i.e. MyApplication" />
  <add key="zipkinSpanProcessorBatchSize" value="10" />
  <add key="zipkinSampleRate" value="0.5" />
  <add key="zipkinNotToBeDisplayedDomainList" value=".xyz.com,.myApplication.net" />
  <add key="zipkinExcludedUriList" value="/check_uri,/status" />
  <add key="zipkinProxyType" value="Http" /> 
</appSettings>
```
	domain - a valid host url string of the host instance. (if not valid, SpanTracer.serviceName = config.zipkinServiceName )

	zipkinScribeServerName - the zipkin scribe/collector server name to connect to send the Spans

	zipkinScribeServerPort - the zipkin scribe/collector server port to connect to send the Spans

	zipkinServiceName- name of your Service that zipkin will use to label the trace

	zipkinSpanProcessorBatchSize - how many Spans should be sent to the zipkin scribe/collector in one go.
	
	zipkinSampleRate - 1 decimal point float value between 0 and 1.  this value will determine randomly if the current request will be traced or not.

	zipkinNotToBeDisplayedDomainList - comma separate domain list, it will be used when logging hostname by excluding these strings in service name attribute
                                 e.g. domain: ".xyz.com", host: "abc.xyz.com" will be logged as "abc" only    

    zipkinExcludedUriList - uri list that is not needed for tracing

    zipkinProxyType - zipkin proxy type i.e. Http, Socks4, Socks4a, Socks5 

#### Additional configuration for HttpModule package

1) web.config 

The following should be added to add the httpModule to your project.

 ```
  <system.webServer>
    <modules>
      <add name="ZipkinRequestContextModule" type="Medidata.ZipkinTracer.HttpModule.ZipkinRequestContextModule" />
    </modules>
  </system.webServer>
  ```

### Usage Examples

Two ways to use .NET Zipkin Tracer Client

1) Under namespace Medidata.ZipkinTracer.HttpModule, please register IHttpModule "ZipkinRequestContextModule" to your web app. This will do server trace automatically.
   Note: To do client trace (Please see below for client trace example) on other parts of your web app, you can access present ITracerClient instance using:
```
(ITracerClient)HttpContext.Current.Items["zipkinClient"];
```

2) To be more flexible on your implementation, you can insert the server trace calls manually by using ITracerClient methods which is found under the Medidata.ZipkinTracer.Core namespace.

- Instantiate your ITracerClient instance globally [context-wide]. (On each beginning of your request)
- Then do event handling on begin and end request of your requests for server trace

```
context.BeginRequest += (sender, args) =>
{
	var traceProvider = new TraceProvider(
        new HttpContextWrapper(HttpContext.Current),
        ConfigurationManager.AppSettings["zipkinExcludedUriList"],
        ConfigurationManager.AppSettings["zipkinSampleRate"];
    var logger = LogManager.GetLogger(GetType());
    ITracerClient zipkinClient = new ZipkinClient(traceProvider, logger);
    HttpContext.Current.Items["zipkinClient"] = zipkinClient;

    var span = zipkinClient.StartServerTrace(HttpContext.Current.Request.Url, HttpContext.Current.Request.HttpMethod);	
    HttpContext.Current.Items["zipkinSpan"] = span;
}

context.EndRequest += (sender, args) =>
{
    var zipkinClient = (ITracerClient)HttpContext.Current.Items["zipkinClient"];
    var span = (Span)HttpContext.Current.Items["zipkinSpan"];
    zipkinClient.EndServerTrace(span);
};
```

#### Client trace example
Trace before and after a remote request call

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

    result = await client.GetAsync(requestUri);

	// end client trace
    tracerClient.EndClientTrace(span);	
}
...
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

## Contributors
ZipkinTracer is (c) Medidata Solutions Worldwide and owned by its major contributors:
* Tomoko Kwan
* [Kenya Matsumoto](https://github.com/kenyamat)
* [Brent Villanueva](https://github.com/bvillanueva-mdsol)
* [Laszlo Schreck](https://github.com/lschreck-mdsol)
