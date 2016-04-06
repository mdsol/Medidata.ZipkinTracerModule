using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Medidata.ZipkinTracer.Models;

namespace Medidata.ZipkinTracer.Core
{
    public class ServiceEndpoint 
    {
        public virtual Endpoint GetLocalEndpoint(string serviceName, ushort port = 443)
        {
            // personally, it should have been nullable short for port. but since zipkin server requires it, 443
            // has be chosen to be that magic number as most servers have it.
            // TODO: get rid of this magic number

            return new Endpoint()
            {
                ServiceName = serviceName,
                IPAddress = GetLocalIPAddress(),
                Port = port
            };
        }

        public virtual Endpoint GetRemoteEndpoint(Uri remoteServer, string remoteServiceName)
        {
            var addressBytes = GetRemoteIPAddress(remoteServer).GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(addressBytes);
            }

            var ipAddressStr = BitConverter.ToInt32(addressBytes, 0);
            var hostIpAddressStr = (int)IPAddress.HostToNetworkOrder(ipAddressStr);

            return new Endpoint()
            {
                ServiceName = remoteServiceName,
                IPAddress = GetRemoteIPAddress(remoteServer),
                Port = (ushort)remoteServer.Port
            };
        }

        private static IPAddress GetLocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        private static IPAddress GetRemoteIPAddress(Uri remoteServer)
        {
            var adressList = Dns.GetHostAddresses(remoteServer.Host);
            return adressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}
