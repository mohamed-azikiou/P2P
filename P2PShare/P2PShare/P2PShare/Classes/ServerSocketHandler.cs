using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Serialization;

namespace P2PShare.Classes
{
    class ServerSocketHandler : SocketHandler
    {
        // Server Socket used only by Server
        private readonly Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public ServerSocketHandler()
        {
            // Bind Server with the EndPoint
            // IPAddress.Any paramiter to Listen on all network interfaces Port is the port
            server.Bind(new IPEndPoint(IPAddress.Any, port));

            // Listen to only one Device
            server.Listen(1);

            // Begin Accept will wait untill a connection comes
            // First parameter is the Action to do when a Connection requested
            // Second parameter is the State it may be used to send some Arguments so they can be retreived when connection requested
            server.BeginAccept((IAsyncResult result) =>
            {
                // Accept request and get Client Socket
                client = server.EndAccept(result);
                Connected = client.Connected;

                // Execute ReceiveData when receiving sizeof(int) Data into buf starting from Zero Index
                client.BeginReceive(buf, 0, sizeof(int), SocketFlags.None, ReceiveData, null);
            }, null);
        }
        public void Dispose()
        {
            client.Close();
            server.Close();
        }
    }
}
