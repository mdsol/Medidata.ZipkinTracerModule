using System.Collections.Generic;

namespace Medidata.ZipkinTracer.Core
{
    public interface IZipkinConfig
    {
        string ZipkinBaseUri { get; }
        string ServiceName { get; }
        string SpanProcessorBatchSize { get; }
        string DontSampleListCsv { get; }
        string ZipkinSampleRate { get; }
        string Domain { get; }
        List<string> GetNotToBeDisplayedDomainList();
    }
}
