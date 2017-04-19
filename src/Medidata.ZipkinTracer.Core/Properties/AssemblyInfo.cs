using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Medidata.ZipkinTracer.Core")]
[assembly: AssemblyDescription("Core package for the Zipkin tracer module from Medidata.")]
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
[assembly: Guid("dfbe97d6-8b81-4bee-b2ce-23ef0f4d9953")]

[assembly: AssemblyVersion("4.0.0")]
[assembly: AssemblyFileVersion("4.0.0")]
[assembly: AssemblyInformationalVersion("4.0.0")]
[assembly: InternalsVisibleTo("Medidata.ZipkinTracer.Tests")]