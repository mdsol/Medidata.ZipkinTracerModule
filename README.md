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

### Config transformations
Below are the config transformations that is needed.  

These transformations will be made automatically if the Medidata.ZipkinTracer.HttpModule nuget package is installed.  These changes will need to be made manually for Medidata.ZipkinTracer.Core nuget package.

1) appsettings.template.config, app.config

Add the below additional configurations. Please verify these values and modify them according to your service/environment.

```
<appSettings>
  <add key="zipkinScribeServerName" value="zipkinvm.cloudapp.net" />
  <add key="zipkinScribeServerPort" value="9410" />
  <add key="zipkinServiceName" value="Name of your Service i.e. MyApplication" />
  <add key="zipkinSpanProcessorBatchSize" value="10" />
  <add key="zipkinSampleRate" value="0.5" />
  <add key="zipkinNotToBeDisplayedDomainList" value=".xyz.com,.myApplication.net" />
  <add key="zipkinExcludedUriList" value="/check_uri,/status" />
</appSettings>
```

	zipkinScribeServerName - the zipkin scribe/collector server name to connect to send the Spans

	zipkinScribeServerPort - the zipkin scribe/collector server port to connect to send the Spans

	zipkinServiceName- name of your Service that zipkin will use to label the trace

	zipkinSpanProcessorBatchSize - how many Spans should be sent to the zipkin scribe/collector in one go.
	
	zipkinSampleRate - 1 decimal point float value between 0 and 1.  this value will determine randomly if the current request will be traced or not.

	zipkinNotToBeDisplayedDomainList - comma separate domain list, it will be used when logging hostname by excluding these strings in service name attribute
                                 e.g. domain: ".xyz.com", host: "abc.xyz.com" will be logged as "abc" only    

    zipkinExcludedUriList - uri list that is not needed for tracing

2) parameters.xml

This is used in opscode's xml when deploying service (i.e. MyApplication) to customize the values to be used in appsettings.

The values are the same as appsettings.template.config

```
<parameters>
  <parameter name="Zipkin Scribe Server Name" description="Zipkin scribe server name" defaultValue="zipkinvm.cloudapp.net">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinScribeServerName']/@value" />
  </parameter>
  <parameter name="Zipkin Scribe Server Port" description="Zipkin scribe server port" defaultValue="9410">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinScribeServerPort']/@value" />
  </parameter>
  <parameter name="Zipkin Service Name" description="Service name to be traced in Zipkin" defaultValue="MyApplication">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinServiceName']/@value" />
  </parameter>
  <parameter name="Zipkin Span Processor Batch Size" description="Number of spans to send to zipkin collector in one go" defaultValue="10">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinSpanProcessorBatchSize']/@value" />
  </parameter>
  <parameter name="Zipkin Sample Rate" description="float between 0 and 1 to determine whether to send a zipkin trace" defaultValue="0.5">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinSampleRate']/@value" />
  </parameter>
  <parameter name="Zipkin Not To Be Displayed Domain List" description="comma separate domain list, it will be used when logging hostname by excluding these strings in service name attribute" defaultValue=".myApplication.net">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinNotToBeDisplayedDomainList']/@value" />
  </parameter>
  <parameter name="Zipkin Excluded Uri List" description="uri list that is not needed for tracing" defaultValue="/check_uri,/status">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinExcludedUriList']/@value" />
  </parameter>
</parameters>
```

#### Config transformations for HttpModule package

1) web.config 

The following will be added to add the httpModule to your project.  Please don't modify this.

 ```
  <system.webServer>
    <modules>
      <add name="ZipkinRequestContextModule" type="Medidata.ZipkinTracerModule.HttpModule.ZipkinRequestContextModule" />
    </modules>
  </system.webServer>
  ```

#### Usage Examples

- Under namespace Medidata.ZipkinTracer.HttpModule, please register IHttpModule "ZipkinRequestContextModule" to your web app.
   Note: To do client trace on other parts of your web app, you can access present ITracerClient instance using:
```
(ITracerClient)HttpContext.Current.Items["zipkinClient"];
```

- To be more flexible on your implementation, you can insert the calls manually by using ITracerClient methods which is found under the Medidata.ZipkinTracer.Core namespace.

First you gotta instantiate your ITracerClient instance globally [context-wide]. (On each beginning of your request)
Then do event handling on begin and end request of your requests.
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

Client trace example, use it by doing a trace before and after a remote request call
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

## Contributors
ZipkinTracer is (c) Medidata Solutions Worldwide and owned by its major contributors:
* Tomoko Kwan
* [Kenya Matsumoto](https://github.com/kenyamat)
* [Brent Villanueva](https://github.com/bvillanueva-mdsol)
