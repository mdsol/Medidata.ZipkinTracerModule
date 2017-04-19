using System;
using System.Collections.Generic;
using System.Linq;
using Medidata.ZipkinTracer.Core;
using Medidata.ZipkinTracer.Core.Logging;
using Medidata.ZipkinTracer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using Rhino.Mocks;

namespace Medidata.ZipkinTracer.Tests
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
        private ushort port;
        private string api;

        [TestInitialize]
        public void Init()
        {
            fixture = new Fixture();
            logger = MockRepository.GenerateStub<ILog>();
            spanCollectorStub = MockRepository.GenerateStub<SpanCollector>(new Uri("http://localhost"), (uint)0);
            zipkinEndpointStub = MockRepository.GenerateStub<ServiceEndpoint>();
            zipkinNotToBeDisplayedDomainList = new List<string> {".xyz.net"};
            serverServiceName = "xyz-sandbox";
            clientServiceName = "abc-sandbox";
            port = 42;
            api = "/api/method1";
        }

        [TestMethod]
        public void CreateNewSpan()
        {
            var spanName = fixture.Create<string>();
            var traceId = Guid.NewGuid().ToString("N");
            var parentSpanId = fixture.Create<long>().ToString("x");
            var spanId = fixture.Create<long>().ToString("x");

            var resultSpan = SpanTracer.CreateNewSpan(spanName, traceId, parentSpanId, spanId);

            Assert.AreEqual(spanName, resultSpan.Name);
            Assert.AreEqual(traceId, resultSpan.TraceId);
            Assert.AreEqual(parentSpanId, resultSpan.ParentId);
            Assert.AreEqual(spanId, resultSpan.Id);
        }

        [TestMethod]
        public void CreateNewSpan_WithNullParentSpanId()
        {
            var resultSpan = SpanTracer.CreateNewSpan(fixture.Create<string>(), fixture.Create<long>().ToString(), null, fixture.Create<long>().ToString());

            Assert.IsNull(resultSpan.ParentId);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullSpanCollector()
        {
            new SpanTracer(null, zipkinEndpointStub, new List<string>(), fixture.Create<Uri>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinEndpoint()
        {
            new SpanTracer(spanCollectorStub, null, new List<string>(), fixture.Create<Uri>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullZipkinNotToBeDomainList()
        {
            new SpanTracer(spanCollectorStub, zipkinEndpointStub, null, fixture.Create<Uri>());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CTOR_WithNullDomain()
        {
            new SpanTracer(spanCollectorStub, zipkinEndpointStub, new List<string>(), null);
        }

        [TestMethod]
        public void ReceiveServerSpan()
        {
            var domain = new Uri("http://server.com");
            var requestName = fixture.Create<string>();
            var traceId = Guid.NewGuid().ToString("N");
            var parentSpanId = fixture.Create<long>().ToString("x");
            var spanId = fixture.Create<long>().ToString("x");
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);

            var localEndpoint = new Endpoint { ServiceName = serverServiceName, Port = port };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(domain.Host), Arg.Is(port))).Return(localEndpoint);

            var resultSpan = spanTracer.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            Assert.AreEqual(requestName, resultSpan.Name);
            Assert.AreEqual(traceId, resultSpan.TraceId);
            Assert.AreEqual(parentSpanId, resultSpan.ParentId);
            Assert.AreEqual(spanId, resultSpan.Id);

            Assert.AreEqual(1, resultSpan.GetAnnotationsByType<Annotation>().Count());

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.IsNotNull(annotation);
            Assert.AreEqual(ZipkinConstants.ServerReceive, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);
            Assert.IsNotNull(annotation.Host);

            Assert.AreEqual(localEndpoint, annotation.Host);

            var binaryAnnotations = resultSpan.GetAnnotationsByType<BinaryAnnotation>();

            Assert.AreEqual(1, binaryAnnotations.Count());

            AssertBinaryAnnotations(binaryAnnotations, "http.path", serverUri.AbsolutePath);
        }

        [TestMethod]
        public void ReceiveServerSpan_UsingToBeCleanedDomainName()
        {
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<Guid>().ToString("N");
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);

            var domain = new Uri("https://" + serverServiceName + zipkinNotToBeDisplayedDomainList.First());

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);

            var localEndpoint = new Endpoint { ServiceName = serverServiceName, Port = port };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(serverServiceName), Arg.Is(port))).Return(localEndpoint);

            var resultSpan = spanTracer.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.AreEqual(localEndpoint, annotation.Host);
        }

        [TestMethod]
        public void ReceiveServerSpan_UsingAlreadyCleanedDomainName()
        {
            var domain = new Uri("https://server.com");
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<Guid>().ToString("N");
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);

            var localEndpoint = new Endpoint { ServiceName = domain.Host, Port = port };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(domain.Host), Arg.Is(port))).Return(localEndpoint);

            var resultSpan = spanTracer.ReceiveServerSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.AreEqual(localEndpoint, annotation.Host);
        }

        [TestMethod]
        public void SendServerSpan()
        {
            var domain = new Uri("https://server.com");
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);

            var endpoint = new Endpoint() { ServiceName = domain.Host, Port = (ushort)domain.Port };
            var expectedSpan = new Span();
            expectedSpan.Annotations.Add(new Annotation() { Host = endpoint, Value = ZipkinConstants.ServerReceive, Timestamp = DateTimeOffset.UtcNow });

            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(domain.Host, (ushort)domain.Port)).Return(new Endpoint() { ServiceName = domain.Host });

            spanTracer.SendServerSpan(expectedSpan);

            spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Matches(y =>
                    ValidateSendServerSpan(y, domain.Host)
                    ))
                );
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SendServerSpan_NullSpan()
        {
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, new Uri("http://server.com"));

            spanTracer.SendServerSpan(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendServerSpan_NullAnnotation()
        {
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, new Uri("http://server.com"));

            var expectedSpan = new Span();

            spanTracer.SendServerSpan(expectedSpan);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void SendServerSpan_InvalidAnnotation()
        {
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, new Uri("http://server.com"));

            var expectedSpan = new Span();

            spanTracer.SendServerSpan(expectedSpan);
        }

        [TestMethod]
        public void SendClientSpan()
        {
            var domain = new Uri("https://server.com");
            var requestName = fixture.Create<string>();
            var traceId = Guid.NewGuid().ToString("N");
            var parentSpanId = fixture.Create<long>().ToString("x");
            var spanId = fixture.Create<long>().ToString("x");
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);

            var localEndpoint = new Endpoint {ServiceName = serverServiceName};
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(domain.Host), Arg<ushort>.Is.Anything)).Return(localEndpoint);
            var remoteEndpoint = new Endpoint { ServiceName = clientServiceName, Port = port };
            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(clientServiceName))).Return(remoteEndpoint);

            var resultSpan = spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            Assert.AreEqual(requestName, resultSpan.Name);
            Assert.AreEqual(traceId, resultSpan.TraceId);
            Assert.AreEqual(parentSpanId, resultSpan.ParentId);
            Assert.AreEqual(spanId, resultSpan.Id);



            Assert.AreEqual(1, resultSpan.GetAnnotationsByType<Annotation>().Count());

            var annotation = resultSpan.Annotations[0] as Annotation;
            Assert.IsNotNull(annotation);
            Assert.AreEqual(ZipkinConstants.ClientSend, annotation.Value);
            Assert.IsNotNull(annotation.Timestamp);
            Assert.AreEqual(localEndpoint, annotation.Host);

            var binaryAnnotations = resultSpan.GetAnnotationsByType<BinaryAnnotation>();

            Assert.AreEqual(2, binaryAnnotations.Count());
            AssertBinaryAnnotations(binaryAnnotations, "http.path", serverUri.AbsolutePath);
            AssertBinaryAnnotations(binaryAnnotations, "sa", "1");

            var endpoint = binaryAnnotations.ToArray()[1].Host as Endpoint;

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(clientServiceName, endpoint.ServiceName);
            Assert.AreEqual(port, endpoint.Port);
        }

        [TestMethod]
        public void SendClientSpanWithDomainUnderFilterList()
        {
            var domain = new Uri("https://server.com");
            var requestName = fixture.Create<string>();
            var traceId = fixture.Create<Guid>().ToString("N");
            var parentSpanId = fixture.Create<long>().ToString();
            var spanId = fixture.Create<long>().ToString();
            var serverUri = new Uri("https://" + clientServiceName + zipkinNotToBeDisplayedDomainList.First() + ":" + port + api);

            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);

            var localEndpoint = new Endpoint { ServiceName = serverServiceName };
            zipkinEndpointStub.Expect(x => x.GetLocalEndpoint(Arg.Is(domain.Host), Arg<ushort>.Is.Anything)).Return(localEndpoint);
            var remoteEndpoint = new Endpoint { ServiceName = clientServiceName, Port = port };
            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(Arg.Is(serverUri), Arg.Is(clientServiceName))).Return(remoteEndpoint);

            var resultSpan = spanTracer.SendClientSpan(requestName, traceId, parentSpanId, spanId, serverUri);

            var endpoint = resultSpan.GetAnnotationsByType<BinaryAnnotation>().ToArray()[1].Host as Endpoint;

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(clientServiceName, endpoint.ServiceName);
            Assert.AreEqual(port, endpoint.Port);
        }

        [TestMethod]
        public void ReceiveClientSpan()
        {
            var domain = new Uri("http://server.com");
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);
            var endpoint = new Endpoint() { ServiceName = clientServiceName, Port = port };
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);
            var returnCode = fixture.Create<short>();
            var expectedSpan = new Span();

            expectedSpan.Annotations.Add(new Annotation() { Host = endpoint, Value = ZipkinConstants.ClientSend, Timestamp = DateTimeOffset.UtcNow });

            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(serverUri, domain.Host)).Return(endpoint);

            spanTracer.ReceiveClientSpan(expectedSpan, returnCode);

            spanCollectorStub.AssertWasCalled(x => x.Collect(Arg<Span>.Matches(y =>
                    ValidateReceiveClientSpan(y, clientServiceName, port)
                    ))
                );
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ReceiveClientSpan_EmptyAnnotationsList()
        {
            var domain = new Uri("http://server.com");
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);
            var endpoint = new Endpoint() { ServiceName = clientServiceName };
            var serverUri = new Uri("https://" + clientServiceName + ":" + port + api);
            var returnCode = fixture.Create<short>();
            var expectedSpan = new Span();

            zipkinEndpointStub.Expect(x => x.GetRemoteEndpoint(serverUri, domain.Host)).Return(endpoint);

            spanTracer.ReceiveClientSpan(expectedSpan, returnCode);
        }

        [TestMethod]
        [TestCategory("TraceRecordTests")]
        public void Record_WithSpanAndValue_AddsNewAnnotation()
        {
            // Arrange
            var expectedDescription = "Description";
            var expectedSpan = new Span();
            var domain = new Uri("http://server.com");
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);

            // Act
            spanTracer.Record(expectedSpan, expectedDescription);
            
            // Assert
            Assert.IsNotNull(
                expectedSpan.GetAnnotationsByType<Annotation>().SingleOrDefault(a => (string)a.Value == expectedDescription),
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
                new { Value = true, ExpectedResult = true, Type = AnnotationType.Boolean },
                new { Value = short.MaxValue, ExpectedResult = short.MaxValue, Type = AnnotationType.Int16 },
                new { Value = int.MaxValue, ExpectedResult = int.MaxValue, Type = AnnotationType.Int32 },
                new { Value = long.MaxValue, ExpectedResult = long.MaxValue, Type = AnnotationType.Int64 },
                new { Value = double.MaxValue, ExpectedResult = double.MaxValue, Type = AnnotationType.Double },
                new { Value = "String", ExpectedResult = "String", Type = AnnotationType.String },
                new { Value = DateTime.MaxValue, ExpectedResult = DateTime.MaxValue, Type = AnnotationType.String }
            };

            var domain = new Uri("http://server.com");
            var spanTracer = new SpanTracer(spanCollectorStub, zipkinEndpointStub, zipkinNotToBeDisplayedDomainList, domain);

            foreach (var testValue in testValues)
            {
                var expectedSpan = new Span();
 
                // Act
                spanTracer.RecordBinary(expectedSpan, keyName, testValue.Value);

                // Assert
                var actualAnnotation = expectedSpan
                    .GetAnnotationsByType<BinaryAnnotation>()?
                    .SingleOrDefault(a => a.Key == keyName);

                var result = actualAnnotation?.Value;
                var annotationType = actualAnnotation?.AnnotationType;
                Assert.AreEqual(testValue.ExpectedResult, result, "The recorded value in the annotation is wrong.");
                Assert.AreEqual(testValue.Type, annotationType, "The Annotation Type is wrong.");
            }
        }

        private bool ValidateReceiveClientSpan(Span y, string serviceName, ushort port)
        {
            var firstannotation = (Annotation)y.Annotations[0];
            var firstEndpoint = (Endpoint)firstannotation.Host;

            Assert.AreEqual(serviceName, firstEndpoint.ServiceName);
            Assert.AreEqual(port, firstEndpoint.Port);
            Assert.AreEqual(ZipkinConstants.ClientSend, firstannotation.Value);
            Assert.IsNotNull(firstannotation.Timestamp);

            var secondAnnotation = (Annotation)y.Annotations[1];
            var secondEndpoint = (Endpoint)secondAnnotation.Host;

            Assert.AreEqual(serviceName, secondEndpoint.ServiceName);
            Assert.AreEqual(port, secondEndpoint.Port);
            Assert.AreEqual(ZipkinConstants.ClientReceive, secondAnnotation.Value);
            Assert.IsNotNull(secondAnnotation.Timestamp);

            return true;
        }

        private bool ValidateSendServerSpan(Span y, string serviceName)
        {
            var firstAnnotation = (Annotation)y.Annotations[0];
            var firstEndpoint = (Endpoint)firstAnnotation.Host;

            Assert.AreEqual(serviceName, firstEndpoint.ServiceName);
            Assert.AreEqual(ZipkinConstants.ServerReceive, firstAnnotation.Value);
            Assert.IsNotNull(firstAnnotation.Timestamp);

            var secondAnnotation = (Annotation)y.Annotations[1];
            var secondEndpoint = (Endpoint)secondAnnotation.Host;

            Assert.AreEqual(serviceName, secondEndpoint.ServiceName);
            Assert.AreEqual(ZipkinConstants.ServerSend, secondAnnotation.Value);
            Assert.IsNotNull(secondAnnotation.Timestamp);

            return true;
        }

        private void AssertBinaryAnnotations(IEnumerable<BinaryAnnotation> list, string key, string value)
        {
            Assert.AreEqual(value, list.Where(x => x.Key.Equals(key)).Select(x => x.Value).First());
        }
    }
}