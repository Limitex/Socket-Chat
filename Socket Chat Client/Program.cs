using System;
using System.Net;
using SocketComunication;

namespace Socket_Chat_Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Socket Chat Client");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            var client = new SocketChat.Client(ipAddress, 11000);
            Console.Write("User Name : ");
            client.Start(Console.ReadLine());
        }
    }
}
