﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Medidata.ZipkinTracerModule
{
    public class ZipkinEndpoint : IZipkinEndpoint
    {
        public Endpoint GetEndpoint(string serviceName)
        {
            var ipAddressStr = BitConverter.ToInt32(GetLocalIPAddress().GetAddressBytes(), 0);
            var hostIpAddressStr = (int) IPAddress.NetworkToHostOrder(ipAddressStr);

            return new Endpoint()
            {
                Service_name = serviceName,
                Ipv4 = hostIpAddressStr,
                Port = 20065
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
    }
}