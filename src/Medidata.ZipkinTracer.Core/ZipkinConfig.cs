using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Medidata.ZipkinTracer.Core
{
    [ExcludeFromCodeCoverage]  //excluded from code coverage since this class are getters using static .net class ConfigurationManager
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
            get {  return ConfigurationManager.AppSettings["mAuthWhitelist"];}
        }

        public string ZipkinSampleRate
        {
            get {  return ConfigurationManager.AppSettings["zipkinSampleRate"];}
        }
    }
}
