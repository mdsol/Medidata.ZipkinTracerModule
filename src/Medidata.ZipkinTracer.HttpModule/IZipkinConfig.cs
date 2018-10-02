using System;
using System.Collections.Generic;

namespace Medidata.ZipkinTracer.HttpModule
{
    public interface IZipkinConfig
    {
        Uri ZipkinBaseUri { get; }
        string ServiceName { get; }
        IEnumerable<string> DontSampleListCsv { get; }
        float ZipkinSampleRate { get; }
    }
}