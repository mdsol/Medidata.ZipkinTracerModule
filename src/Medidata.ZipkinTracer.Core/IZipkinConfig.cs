using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medidata.ZipkinTracer.Core
{
    public interface IZipkinConfig
    {
        string ZipkinServerName { get; }
        string ZipkinServerPort { get; }
        string ServiceName { get; }
        string SpanProcessorBatchSize { get; }
        string DontSampleListCsv { get; }
        string ZipkinSampleRate { get; }

        /// <summary>
        /// comma separate domain list from config formatted to string list
        /// will be used to exclude this strings when logging hostname as service name
        /// e.g. domain: ".xyz.com", host: "abc.xyz.com" will be logged as abc
        /// </summary>
        /// <returns></returns>
        List<string> GetInternalDomainList();
    }
}
