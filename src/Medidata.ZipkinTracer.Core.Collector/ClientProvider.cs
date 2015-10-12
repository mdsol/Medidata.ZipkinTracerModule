using Starksoft.Aspen.Proxy;
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
        private ProxyType proxyType;
        private TTransport transport;
        private ZipkinCollector.Client client;
        private static ClientProvider instance;

        private ClientProvider(string host, int port, Uri proxyServer = null, string proxyType = null) 
        {
            this.host = host;
            this.port = port;
            if (proxyServer != null && proxyServer.Host != host && proxyServer.Port != port)
            {
                this.proxyServer = proxyServer;
            }

            this.proxyType = ProxyType.None;
            Enum.TryParse(proxyType, out this.proxyType);
        }

        public static ClientProvider GetInstance(string host, int port, Uri proxyServer = null, string proxyType = null)
        {
            if (instance == null)
            {
                instance = new ClientProvider(host, port, proxyServer, proxyType);
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
            if (proxyServer != null)
            {
                var proxy = new ProxyClientFactory().CreateProxyClient(proxyType, proxyServer.Host, proxyServer.Port);
                proxy.TcpClient = new TcpClient(host, port);
                zipkinTcpClient = proxy.TcpClient;
            }
            else
            {
                zipkinTcpClient = new TcpClient(host, port);
            }
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
