using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;

namespace Medidata.ZipkinTracerModule.Collector
{
    public class ClientProvider : IClientProvider
    {
        private readonly string host;
        private readonly int port;
        private TTransport transport;

        public ZipkinCollector.Client Client { get; private set; }

        public ClientProvider(string host, int port) 
        {
            this.host = host;
            this.port = port;
        }

        public void Setup()
        {
            var socket = new TSocket(host, port);
            transport = new TFramedTransport(socket);
            var protocol = new TBinaryProtocol(transport);
            Client = new ZipkinCollector.Client(protocol);
            transport.Open();
        }

        public void Close()
        {
            if (transport != null)
            {
                transport.Close();
                transport.Dispose();
            }
        }
    }
}
