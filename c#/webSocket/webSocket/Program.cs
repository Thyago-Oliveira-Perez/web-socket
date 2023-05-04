using System.Net.Sockets;
using System.Net;
using System;

namespace webSocket
{
    class Server
    {
        public static void Main()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);
            server.Start();

            Console.WriteLine("Server has started on 127.0.0.1:80.\nWaiting for a connection...");

            TcpClient client = server.AcceptTcpClient();

            Console.WriteLine("A client connected.");
        }
    }
}