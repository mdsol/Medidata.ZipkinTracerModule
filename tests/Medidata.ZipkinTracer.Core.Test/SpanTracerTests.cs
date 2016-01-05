using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Medidata.ZipkinTracer.Core.Collector;
using Rhino.Mocks;
using System.Linq;
using System.Collections.Generic;
using log4net;

namespace Medidata.ZipkinTracer.Core.Test
{
    [TestClass]
    public class SpanTracerTests
    {
        private IFixture fixture;
        private SpanCollector spanCollectorStub;
        private ServiceEndpoint zipkinEndpointStub;
        private ILog logger;
        private IEnumerable<string> zipkinNotToBeDisplayedDomainList;
        private string serverServiceName;
        private string clientServiceName;
        private short port;
        private string api;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            logger = MockRepository.GenerateStub<ILog>();
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), 0, logger);
            zipkinEndpointStub = MockRepository.GenerateStub<ServiceEndpoint>();
            zipkinNotToBeDisplayedDomainList = new List<string> {".xyz.net"};
            serverServiceName = "xyz-sandbox";
            clientServiceName = "abc-sandbox";
            port = 42;
            api = "/api/method1";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanCollector()
        {
            new SpanTracer(null, zipkinEndpointStub, null, fixture.Create<string>(), fixture.Create<string>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullOrEmptyString()
        {
            new SpanTracer(spanCollectorStub, zipkinEndpointStub, null, fixture.Create<string>(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinEndpoint()
        {
            new SpanTracer(spanCollectorStub, null, null, fixture.Create<string>(), fixture.Create<string>());
        }

        [TestMethod]
        public void ReceiveServerSpan()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            var localEndpoint = new Endpoint { Service_name = serverServiceName, Port = port };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(serviceName), Arg.Is(port))).Return(localEndpoint);

            var resultSpan = spanTracer.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            Assert.AreEqual(requestName, resultSpan.Name);
            Assert.AreEqual(Int64.Parse(traceId, System.Globalization.NumberStyles.HexNumber), resultSpan.Trace_id);
            Assert.AreEqual(Int64.Parse(parentSpanId, System.Globalization.NumberStyles.HexNumber), resultSpan.Parent_id);
            Assert.AreEqual(Int64.Parse(spanId, System.Globalization.NumberStyles.HexNumber), resultSpan.Id);

            Assert.AreEqual(1, resultSpan.Annotations.Count);

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.IsNotNull(annotation);
            Assert.AreEqual(zipkinCoreConstants.SERVER_RECV, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);
            Assert.IsNotNull(annotation.Host);

            Assert.AreEqual(localEndpoint, annotation.Host);

            Assert.AreEqual(1, resultSpan.Binary_annotations.Count);
            AssertBinaryAnnotations(resultSpan.Binary_annotations, "http.uri", serverUri.AbsolutePath);
        }

        [TestMethod]
        public void ReceiveServerSpan_UsingCleanedDomainName()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);

            var domain = serverServiceName + zipkinNotToBeDisplayedDomainList.First();

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain, serviceName);

            var localEndpoint = new Endpoint { Service_name = serverServiceName, Port = port };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(serverServiceName), Arg.Is(port))).Return(localEndpoint);

            var resultSpan = spanTracer.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.AreEqual(localEndpoint, annotation.Host);
        }

        [TestMethod]
        public void ReceiveServerSpan_UsingDomainName()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);

            var domain = serverServiceName;

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain, serviceName);

            var localEndpoint = new Endpoint { Service_name = serverServiceName, Port = port };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(serverServiceName), Arg.Is(port))).Return(localEndpoint);

            var resultSpan = spanTracer.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.AreEqual(localEndpoint, annotation.Host);
        }

        [TestMethod]
        public void SendServerSpan()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            var endpoint = new Endpoint() { Service_name = serviceName };
            var expectedSpan = new Span() { Annotations = new List<Annotation>() };
            expectedSpan.Annotations.Add(new Annotation() { Host = endpoint, Value = zipkinCoreConstants.SERVER_RECV, Timestamp = 1 });

            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(serviceName)).Return(new Endpoint() { Service_name = serviceName });

            spanTracer.SendServerSpan(expectedSpan);

            spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Matches(y =>
                    ValidateSendServerSpan(y, serviceName)
                    ))
                );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendServerSpan_NullSpan()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            spanTracer.SendServerSpan(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendServerSpan_NullAnnotation()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            var expectedSpan = new Span();

            spanTracer.SendServerSpan(expectedSpan);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendServerSpan_InvalidAnnotation()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            var expectedSpan = new Span() { Annotations = new List<Annotation>() };

            spanTracer.SendServerSpan(expectedSpan);
        }

        [TestMethod]
        public void SendClientSpan()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            var localEndpoint = new Endpoint {Service_name = serverServiceName};
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(serviceName), Arg<short>.Is.Anything)).Return(localEndpoint);
            var remoteEndpoint = new Endpoint { Service_name = clientServiceName };
            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(clientServiceName))).Return(remoteEndpoint);

            var resultSpan = spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            Assert.AreEqual(requestName, resultSpan.Name);
            Assert.AreEqual(Int64.Parse(traceId, System.Globalization.NumberStyles.HexNumber), resultSpan.Trace_id);
            Assert.AreEqual(Int64.Parse(parentSpanId, System.Globalization.NumberStyles.HexNumber), resultSpan.Parent_id);
            Assert.AreEqual(Int64.Parse(spanId, System.Globalization.NumberStyles.HexNumber), resultSpan.Id);

            Assert.AreEqual(1, resultSpan.Annotations.Count);

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.IsNotNull(annotation);
            Assert.AreEqual(zipkinCoreConstants.CLIENT_SEND, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);
            Assert.AreEqual(localEndpoint, annotation.Host);
            
            Assert.AreEqual(2, resultSpan.Binary_annotations.Count);
            AssertBinaryAnnotations(resultSpan.Binary_annotations, "http.uri", serverUri.AbsolutePath);
            AssertBinaryAnnotations(resultSpan.Binary_annotations, "sa", "1");

            var endpoint = resultSpan.Binary_annotations[1].Host as Endpoint;
            Assert.IsNotNull(endpoint);
            Assert.AreEqual(clientServiceName, endpoint.Service_name);
        }

        [TestMethod]
        public void SendClientSpanWithDomainUnderFilterList()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var serverUri = new Uri("https://" + clientServiceName + zipkinNotToBeDisplayedDomainList.First() + ":" + port + api);

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            var localEndpoint = new Endpoint { Service_name = serverServiceName };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(serviceName), Arg<short>.Is.Anything)).Return(localEndpoint);
            var remoteEndpoint = new Endpoint { Service_name = clientServiceName };
            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(clientServiceName))).Return(remoteEndpoint);

            var resultSpan = spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            var endpoint = resultSpan.Binary_annotations[1].Host as Endpoint;
            Assert.IsNotNull(endpoint);
            Assert.AreEqual(clientServiceName, endpoint.Service_name);
        }

        [TestMethod]
        public void SendClientSpanWithDomainNotUnderFilterList()
        {
            var serviceName = fixture.Create<string>();
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<long>().ToString();
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var domain = ".hij.com";
            var serverUri = new Uri("https://" + clientServiceName + domain + ":" + port + api);

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            var localEndpoint = new Endpoint { Service_name = serverServiceName };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(serviceName), Arg<short>.Is.Anything)).Return(localEndpoint);
            var remoteEndpoint = new Endpoint { Service_name = clientServiceName + domain };
            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(clientServiceName + domain))).Return(remoteEndpoint);

            var resultSpan = spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            var endpoint = resultSpan.Binary_annotations[1].Host as Endpoint;
            Assert.IsNotNull(endpoint);
            Assert.AreEqual(clientServiceName + domain, endpoint.Service_name);
        }

        [TestMethod]
        public void ReceiveClientSpan()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);
            var endpoint = new Endpoint() { Service_name = clientServiceName };
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);
            var returnCode = fixture.Create<short>();
            var expectedSpan = new Span()
            {
                Annotations = new List<Annotation>(),
                Binary_annotations = new List<BinaryAnnotation>()
            };
            expectedSpan.Annotations.Add(new Annotation() { Host = endpoint, Value = zipkinCoreConstants.CLIENT_SEND, Timestamp = 1 });

            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(serverUri, serviceName)).Return(endpoint);

            spanTracer.ReceiveClientSpan(expectedSpan, returnCode);

            spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Matches(y =>
                    ValidateReceiveClientSpan(y, clientServiceName)
                    ))
                );
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceiveClientSpan_NullAnnotation()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);
            var endpoint = new Endpoint() { Service_name = clientServiceName };
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);
            var returnCode = fixture.Create<short>();
            var expectedSpan = new Span()
            {
                Annotations = null
            };

            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(serverUri, serviceName)).Return(endpoint);

            spanTracer.ReceiveClientSpan(expectedSpan, returnCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceiveClientSpan_EmptyAnnotationsList()
        {
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);
            var endpoint = new Endpoint() { Service_name = clientServiceName };
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);
            var returnCode = fixture.Create<short>();
            var expectedSpan = new Span()
            {
                Annotations = new List<Annotation>()
            };

            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(serverUri, serviceName)).Return(endpoint);

            spanTracer.ReceiveClientSpan(expectedSpan, returnCode);
        }

        [TestMethod]
        [TestCategory("TraceRecordTests")]
        public void Record_WithSpanAndValue_AddsNewAnnotation()
        {
            // Arrange
            var expectedDescription = "Description";
            var expectedSpan = new Span() { Annotations = new List<Annotation>() };
            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            // Act
            spanTracer.Record(expectedSpan, expectedDescription);
            
            // Assert
            Assert.IsNotNull(
                expectedSpan.Annotations.SingleOrDefault(a => a.Value == expectedDescription),
                "The record is not found in the Annotations."
            );
        }

        [TestMethod]
        [TestCategory("TraceRecordTests")]
        public void RecordBinary_WithSpanAndValue_AddsNewTypeCorrectBinaryAnnotation()
        {
            // Arrange
            var keyName = "TestKey";
            var testValues = new dynamic[]
            {
                new { Value = true, Result = "True", Type = AnnotationType.BOOL },
                new { Value = short.MaxValue, Result = short.MaxValue.ToString(), Type = AnnotationType.I16 },
                new { Value = int.MaxValue, Result = int.MaxValue.ToString(), Type = AnnotationType.I32 },
                new { Value = long.MaxValue, Result = long.MaxValue.ToString(), Type = AnnotationType.I64 },
                new { Value = double.MaxValue, Result = double.MaxValue.ToString(), Type = AnnotationType.DOUBLE },
                new { Value = "String", Result = "String", Type = AnnotationType.STRING },
                new { Value = DateTime.MaxValue, Result = DateTime.MaxValue.ToString(), Type = AnnotationType.STRING }
            };

            var serviceName = fixture.Create<string>();
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, null, serviceName);

            foreach (var testValue in testValues)
            {
                var expectedSpan = new Span() { Binary_annotations = new List<BinaryAnnotation>() };
 
                // Act
                spanTracer.RecordBinary(expectedSpan, keyName, testValue.Value);

                // Assert
                var actualAnnotation = expectedSpan.Binary_annotations.SingleOrDefault(a => a.Key == keyName);
                var result = actualAnnotation?.Value;
                var annotationType = actualAnnotation?.Annotation_type;
                Assert.AreEqual(testValue.Result, result, "The recorded value in the annotation is wrong.");
                Assert.AreEqual(testValue.Type, annotationType, "The Annotation Type is wrong.");
            }
        }

        private bool ValidateReceiveClientSpan(Span y, string serviceName)
        {
            var firstannotation = (Annotation)y.Annotations[0];
            var firstEndpoint = (Endpoint)firstannotation.Host;

            Assert.AreEqual(serviceName, firstEndpoint.Service_name);
            Assert.AreEqual(zipkinCoreConstants.CLIENT_SEND, firstannotation.Value);
            Assert.IsNotNull(firstannotation.Timestamp);

            var secondAnnotation = (Annotation)y.Annotations[1];
            var secondEndpoint = (Endpoint)secondAnnotation.Host;

            Assert.AreEqual(serviceName, secondEndpoint.Service_name);
            Assert.AreEqual(zipkinCoreConstants.CLIENT_RECV, secondAnnotation.Value);
            Assert.IsNotNull(secondAnnotation.Timestamp);

            return true;
        }

        private bool ValidateSendServerSpan(Span y, string serviceName)
        {
            var firstAnnotation = (Annotation)y.Annotations[0];
            var firstEndpoint = (Endpoint)firstAnnotation.Host;

            Assert.AreEqual(serviceName, firstEndpoint.Service_name);
            Assert.AreEqual(zipkinCoreConstants.SERVER_RECV, firstAnnotation.Value);
            Assert.IsNotNull(firstAnnotation.Timestamp);

            var secondAnnotation = (Annotation)y.Annotations[1];
            var secondEndpoint = (Endpoint)secondAnnotation.Host;

            Assert.AreEqual(serviceName, secondEndpoint.Service_name);
            Assert.AreEqual(zipkinCoreConstants.SERVER_SEND, secondAnnotation.Value);
            Assert.IsNotNull(secondAnnotation.Timestamp);

            return true;
        }

        private void AssertBinaryAnnotations(List<BinaryAnnotation> list, string key, string value)
        {
            Assert.AreEqual(value, list.Where(x => x.Key.Equals(key)).Select(x => x.Value).First());
        }
    }
}
