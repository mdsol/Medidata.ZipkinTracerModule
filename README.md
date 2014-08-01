# Medidata.ZipkinTracerModule
A .NET implementation of the Zipkin Tracer client.

## Overview
This nuget package implements the zipkin tracer client for .net applications.  Includes a HttpModule which injects zipkin trace into every request which has a zipkin traceId in the header. 

### Config transformations
3 config files will be automatically updated when installing this nuget package. 
1) web.config 
	The following will be added to add the httpModule to your project.  Please don't modify this.
	'''
  <system.webServer>
    <modules>
      <add name="ZipkinRequestContextModule" type="Medidata.ZipkinTracerModule.HttpModule.ZipkinRequestContextModule" />
    </modules>
  </system.webServer>
  '''

2) appsettings.template.config
	Add 4 additional configurations. Please verify these values and modify them according to your service/environment.
	'''
<appSettings>
  <add key="zipkinScribeServerName" value="zipkinvm.cloudapp.net" />
  <add key="zipkinScribeServerPort" value="9410" />
  <add key="ServiceName" value="Name of your Service i.e.Gambit" />
  <add key="spanProcessorBatchSize" value="10" />
</appSettings>
	'''
	zipkinScribeServerName - the zipkin scribe/collector server name to connect to send the Spans
	zipkinScribeServerPort - the zipkin scribe/collector server port to connect to send the Spans
	ServiceName- name of your Service that zipkin will use to label the trace
	spanProcessorBatchSize - how many Spans should be sent to the zipkin scribe/collector in one go.
	
3) parameters.xml

	This is used in opscode's xml when deploying service (i.e. Gambit) to customize the values to be used in appsettings.
	The values are the same as appsettings.template.config

'''
<parameters>
  <parameter name="Zipkin Scribe Server Name" description="Zipkin scribe server name" defaultValue="zipkinvm.cloudapp.net">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinScribeServerName']/@value" />
  </parameter>
  <parameter name="Zipkin Scribe Server Port" description="Zipkin scribe server port" defaultValue="9410">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='zipkinScribeServerPort']/@value" />
  </parameter>
  <parameter name="Service Name" description="Service name to be traced in Zipkin" defaultValue="Gambit">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='ServiceName']/@value" />
  </parameter>
  <parameter name="Span Processor Batch Size" description="Number of spans to send to zipkin collector in one go" defaultValue="10">
    <parameterEntry kind="XmlFile" scope="\\appsettings.config$" match="//appSettings/add[@key='spanProcessorBatchSize']/@value" />
  </parameter>
</parameters>
'''

### Contact
sauce-forge@msdol.com
	 

	 

