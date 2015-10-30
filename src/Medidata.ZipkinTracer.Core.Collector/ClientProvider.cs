using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Thrift.Protocol;
using Thrift.Transport;

namespace Medidata.ZipkinTracer.Core.Collector
{
    public class ClientProvider : IClientProvider
    {
        private readonly string host;
        private readonly int port;
        private Uri proxyServer;
        private TTransport transport;
        private ZipkinCollector.Client client;
        private static ClientProvider instance;

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

        private void Setup()
        {
            var socket = GetProxyEnabledSocket();
            transport = new TFramedTransport(socket);
            var protocol = new TBinaryProtocol(transport);
            client = new ZipkinCollector.Client(protocol);
        }

        private TSocket GetProxyEnabledSocket()
        {

            TcpClient zipkinTcpClient;
            zipkinTcpClient = new TcpClient(host, port);
            return new TSocket(zipkinTcpClient);
        }

        public void Close()
        {
            if (transport != null)
            {
                transport.Close();
                transport.Dispose();
            }
        }

        public void Log(List<LogEntry> logEntries)
        {
            try
            {
                Setup();
                client.Log(logEntries);
            }
            finally
            {
                Close();
            }
        }
    }
}
