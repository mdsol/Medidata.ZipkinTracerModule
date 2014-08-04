using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Thrift.Protocol;
using Thrift.Transport;

namespace Medidata.ZipkinTracerModule.Collector
{
    public class ClientProvider : IClientProvider
    {
        private readonly string host;
        private readonly string proxyHost;
        private readonly int port;
        private readonly int? proxyPort;
        private TTransport transport;

        public ZipkinCollector.Client Client { get; private set; }

        public ClientProvider(string host, int port, string proxyHost = null, int? proxyPort = null) 
        {
            this.host = host;
            this.port = port;
            this.proxyHost = proxyHost;
            this.proxyPort = proxyPort;
        }

        public void Setup()
        {
            var socket = GetProxyEnabledSocket();
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

        public void Log(List<LogEntry> logEntries)
        {
            Client.Log(logEntries);
        }

        private TSocket GetProxyEnabledSocket()
        {
            TcpClient zipkinTcpClient = null;
            if (!string.IsNullOrEmpty(proxyHost) && proxyPort.HasValue)
            {
                var proxy = new Proxy.Socks5ProxyClient(proxyHost, proxyPort.Value);
                zipkinTcpClient = proxy.CreateConnection(host, port);
            }
            else
            {
                zipkinTcpClient = new TcpClient(host, port);
            }
            return new TSocket(zipkinTcpClient);
        }
    }
}
