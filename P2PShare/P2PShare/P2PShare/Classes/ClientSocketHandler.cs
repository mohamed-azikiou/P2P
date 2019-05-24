using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace P2PShare.Classes
{
    class ClientSocketHandler : SocketHandler
    {
        public ClientSocketHandler(string ip)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Task.Run(() =>
                client.BeginConnect(IPAddress.Parse(ip), port, (IAsyncResult result) =>
                {
                    client.EndConnect(result);
                    Connected = client.Connected;
                }, null)
            );
        }
    }
}
