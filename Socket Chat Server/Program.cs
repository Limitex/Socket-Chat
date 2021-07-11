using System;
using System.Net;
using SocketComunication;


namespace Socket_Chat_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Socket Chat Server");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            var server = new SocketChat.Server(ipAddress, 11000);
            server.Start();
        }
    }
}
