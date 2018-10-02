using System;
using System.Collections.Generic;
using System.Configuration;

namespace Medidata.ZipkinTracer.HttpModule
{
    public class ZipkinConfig : IZipkinConfig
    {
        public Uri ZipkinBaseUri
        {
            get { return new Uri(Get("zipkinBaseUri"), UriKind.Absolute); }
        }

        public string ServiceName
        {
            get { return Get("zipkinServiceName"); }
        }

        public IEnumerable<string> DontSampleListCsv
        {
            get { return Array.ConvertAll(Get("zipkinExcludedUriList").Split(','), p => p.Trim()); }
        }

        public float ZipkinSampleRate
        {
            get {  return float.Parse(Get("zipkinSampleRate")); }
        }

        private string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}