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

        private ZipkinCollector.Client Client;

        private static ClientProvider instance = null;

        private ClientProvider(string host, int port) 
        {
            this.host = host;
            this.port = port;
        }

        public static ClientProvider GetInstance(string host, int port)
        {
            if (instance == null)
            {
                instance = new ClientProvider(host, port);
            }
            return instance;
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
            instance = null;
        }

        public void Log(List<LogEntry> logEntries)
        {
            Client.Log(logEntries);
        }
    }
}
