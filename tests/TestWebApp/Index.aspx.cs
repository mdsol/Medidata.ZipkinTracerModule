using System;
using System.IO;
using System.Web;
using System.Web.UI;
using log4net;
using Medidata.ZipkinTracer.Core;

namespace TestWebApp
{
    public partial class Index : Page
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Index));
        
        protected void Page_Load(object sender, EventArgs e)
        {
            return;
        }
    }
}