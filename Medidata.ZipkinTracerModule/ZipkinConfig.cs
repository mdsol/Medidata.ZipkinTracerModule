using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Medidata.ZipkinTracerModule
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
    }
}
