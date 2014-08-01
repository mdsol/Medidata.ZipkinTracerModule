using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Medidata.ZipkinTracerModule
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
    }
}
