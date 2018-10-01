using System;
using System.Linq;
using System.Web;
using zipkin4net;

namespace Medidata.ZipkinTracer.HttpModule
{
    public class ZipkinRequestContextModule : IHttpModule
    {
        private const string Key = nameof(ZipkinRequestContextModule);
        private IZipkinConfig _config;
        private IContextHelper _contextHelper;
        private ITraceHelper _traceHelper;
        
        public ZipkinRequestContextModule()
            : this(new ZipkinConfig(), new ContextHelper(), new TraceHelper())
        {
        }

        public ZipkinRequestContextModule(IZipkinConfig config, IContextHelper contextHelper,
            ITraceHelper traceHelper = null)
        {
            _config = config;
            _contextHelper = contextHelper;
            _traceHelper = traceHelper;
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += BeginRequest;
            context.EndRequest += EndRequest;
        }

        public void BeginRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            var request = _contextHelper.GetRequest(application.Context);
            var response = _contextHelper.GetResponse(application.Context);
            if (IsInDontSampleList(request.Path))
            {
                return;
            }

            var trace = _traceHelper.CreateTrace(request.Headers);
            var serverTrace = _traceHelper.CreateServerTrace(_config.ServiceName, request.HttpMethod);
            _traceHelper.RecordTag(trace, "http.host", request.Url.Authority);
            _traceHelper.RecordTag(trace, "http.uri", request.Url.ToString());
            _traceHelper.RecordTag(trace, "http.path", request.Path);
            _contextHelper.GetItems(application.Context)[Key] = serverTrace;
        }

        public void EndRequest(object sender, EventArgs e)
        {
            var application = (HttpApplication)sender;
            var serverTrace = (ServerTrace)_contextHelper.GetItems(application.Context)[Key];
            serverTrace?.Dispose();
        }

        public void Dispose()
        {
        }

        private bool IsInDontSampleList(string path)
        {
            return path != null &&
                _config.DontSampleListCsv.Any(uri =>
                    path.StartsWith(uri, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}