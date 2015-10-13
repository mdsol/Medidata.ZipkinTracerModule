using System;
using System.Collections.Generic;

namespace Medidata.ZipkinTracer.Core
{
    public interface IZipkinConfig
    {
        string ZipkinServerName { get; }
        string ZipkinServerPort { get; }
        Uri ZipkinProxyServer { get; }
        string ZipkinProxyType { get; }
        string ServiceName { get; }
        string SpanProcessorBatchSize { get; }
        string DontSampleListCsv { get; }
        string ZipkinSampleRate { get; }
        List<string> GetNotToBeDisplayedDomainList();
    }
}
