using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Medidata.ZipkinTracer.Core
{
    public class ZipkinConfig : IZipkinConfig
    {
        public string ServiceName
        {
            get {  return ConfigurationManager.AppSettings["ServiceName"];}
        }

        public string ZipkinServerName
        {
            get { return ConfigurationManager.AppSettings["zipkinScribeServerName"]; }
        }

        public string ZipkinServerPort
        {
            get {  return ConfigurationManager.AppSettings["zipkinScribeServerPort"];}
        }

        public string SpanProcessorBatchSize
        {
            get {  return ConfigurationManager.AppSettings["spanProcessorBatchSize"];}
        }

        public string DontSampleListCsv
        {
            get {  return ConfigurationManager.AppSettings["uriBlacklist"];} // TODO: refactor this later if it is being used
        }

        public string ZipkinSampleRate
        {
            get {  return ConfigurationManager.AppSettings["zipkinSampleRate"];}
        }

        public List<string> GetNotToBeDisplayedDomainList()
        {
            var internalDomainList = new List<string>();

            var rawInternalDomainList = ConfigurationManager.AppSettings["notToBeDisplayedDomainList"];
            if (!string.IsNullOrWhiteSpace(rawInternalDomainList))
            {
                internalDomainList.AddRange(rawInternalDomainList.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.Trim()).ToList());
            }

            return internalDomainList;
        }
    }
}
