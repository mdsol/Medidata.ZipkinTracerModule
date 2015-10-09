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
            get { return ConfigurationManager.AppSettings["zipkinServiceName"]; }
        }

        public string ZipkinServerName
        {
            get { return ConfigurationManager.AppSettings["zipkinScribeServerName"]; }
        }

        public string ZipkinServerPort
        {
            get {  return ConfigurationManager.AppSettings["zipkinScribeServerPort"];}
        }

        public string ZipkinProxyType
        {
            get { return ConfigurationManager.AppSettings["zipkinProxyType"]; }
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

        public Uri GetZipkinProxyServer()
        {
            var proxy = System.Web.Configuration.WebConfigurationManager.GetSection("system.net/defaultProxy") as System.Net.Configuration.DefaultProxySection;
            if (proxy != null && proxy.Enabled)
            {
                return proxy.Proxy.ProxyAddress;
            }
            return null;
        }
    }
}
