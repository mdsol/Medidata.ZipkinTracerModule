using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Medidata.ZipkinTracer.WebApi")]
[assembly: AssemblyDescription("ASP.NET WebApi-specific functionality for the Zipkin tracer module from Medidata.")]
[assembly: AssemblyCompany("Medidata Solutions, Inc.")]
[assembly: AssemblyProduct("Medidata.ZipkinTracerModule")]
[assembly: AssemblyCopyright("Copyright © Medidata Solutions, Inc. 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: ComVisible(false)]
[assembly: Guid("ba59dd84-ef64-45dc-bc60-5cf3ed7929c7")]

[assembly: AssemblyVersion("4.0.0")]
[assembly: AssemblyFileVersion("4.0.0")]
[assembly: AssemblyInformationalVersion("4.0.0")]

[assembly: InternalsVisibleTo("Medidata.ZipkinTracer.Tests")]
