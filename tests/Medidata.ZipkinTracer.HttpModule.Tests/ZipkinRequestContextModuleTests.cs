using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Web;
using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using zipkin4net;

namespace Medidata.ZipkinTracer.HttpModule.Tests
{
    [TestClass]
    public class ZipkinRequestContextModuleTests
    {
        private IFixture _fixture;
        private IZipkinConfig _config;
        private IContextHelper _contextHelper;
        private ITraceHelper _traceHelper;

        [TestInitialize]
        public void Init()
        {
            _fixture = new Fixture();
            _config = MockRepository.GenerateStub<IZipkinConfig>();
            _contextHelper = MockRepository.GenerateStub<IContextHelper>();
            _traceHelper = MockRepository.GenerateStub<ITraceHelper>();
        }

        [TestMethod]
        public void BeginRequest()
        {
            // Arrange
            var serviceName = _fixture.Create<string>();
            var application = new HttpApplication();
            _config.Stub(x => x.ServiceName).Return(serviceName);
            _config.Stub(x => x.DontSampleListCsv).Return(Enumerable.Empty<string>());
            var request = MockRepository.GenerateStub<HttpRequestBase>();
            var path = "api/test";
            var requestUri = new Uri("http://localhost/" + path);
            var headers = new NameValueCollection();
            request.Stub(x => x.Path).Return(path);
            request.Stub(x => x.Url).Return(requestUri);
            request.Stub(x => x.Headers).Return(headers);
            var items = new Hashtable();
            _contextHelper.Stub(x => x.GetRequest(Arg<HttpContext>.Is.Anything)).Return(request);
            _contextHelper.Stub(x => x.GetItems(Arg<HttpContext>.Is.Anything)).Return(items);
            var trace = Trace.Create();
            _traceHelper.Stub(x => x.CreateTrace(headers)).Return(trace);
            _traceHelper.Stub(x => x.RecordTag(Arg<Trace>.Is.Equal(trace), Arg<string>.Is.Anything, Arg<string>.Is.Anything));
            var serverTrace = new ServerTrace(serviceName, HttpMethod.Get.ToString());
            _traceHelper.Stub(x => x.CreateServerTrace(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(serverTrace);
            var sut = new ZipkinRequestContextModule(_config, _contextHelper, _traceHelper);

            // Act
            sut.BeginRequest(application, null);

            // Assert
            var actualTrace = (ServerTrace)items[nameof(ZipkinRequestContextModule)];
            Assert.AreEqual(serverTrace.Trace, actualTrace.Trace);
            _traceHelper.AssertWasCalled(x => x.RecordTag(trace, "http.host", requestUri.Authority));
            _traceHelper.AssertWasCalled(x => x.RecordTag(trace, "http.uri", requestUri.ToString()));
            _traceHelper.AssertWasCalled(x => x.RecordTag(trace, "http.path", path));
        }

        [TestMethod]
        public void BeginRequest_WhenPathIsInWhitelist()
        {
            // Arrange
            var serviceName = _fixture.Create<string>();
            var application = new HttpApplication();
            var request = MockRepository.GenerateStub<HttpRequestBase>();
            var path = "api/test";
            var requestUri = new Uri("http://localhost/" + path);
            var headers = new NameValueCollection();
            request.Stub(x => x.Path).Return(path);
            _config.Stub(x => x.DontSampleListCsv).Return(new[] { path });
            var items = new Hashtable();
            _contextHelper.Stub(x => x.GetRequest(Arg<HttpContext>.Is.Anything)).Return(request);
            var sut = new ZipkinRequestContextModule(_config, _contextHelper, _traceHelper);

            // Act
            sut.BeginRequest(application, null);

            // Assert
            var actualTrace = (ServerTrace)items[nameof(ZipkinRequestContextModule)];
            Assert.IsNull(actualTrace);
        }
    }
}