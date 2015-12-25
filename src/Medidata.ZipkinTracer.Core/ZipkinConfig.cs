using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinConfig : IZipkinConfig
    {
        public string ZipkinBaseUri
        {
            get { return ConfigurationManager.AppSettings["zipkinBaseUri"]; }
        }

        public string ServiceName
        {
            get { return ConfigurationManager.AppSettings["zipkinServiceName"]; }
        }

        public string SpanProcessorBatchSize
        {
            get { return ConfigurationManager.AppSettings["zipkinSpanProcessorBatchSize"]; }
        }

        public string DontSampleListCsv
        {
            get { return ConfigurationManager.AppSettings["zipkinExcludedUriList"]; }
        }

        public string ZipkinSampleRate
        {
            get {  return ConfigurationManager.AppSettings["zipkinSampleRate"];}
        }

        public string Domain
        {
            get { return ConfigurationManager.AppSettings["domain"]; }
        }

        public List<string> GetNotToBeDisplayedDomainList()
        {
            var zipkinNotToBeDisplayedDomainList = new List<string>();

            var rawInternalDomainList = ConfigurationManager.AppSettings["zipkinNotToBeDisplayedDomainList"];
            if (!string.IsNullOrWhiteSpace(rawInternalDomainList))
            {
                zipkinNotToBeDisplayedDomainList.AddRange(rawInternalDomainList.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList());
            }

            return zipkinNotToBeDisplayedDomainList;
        }
    }
}
