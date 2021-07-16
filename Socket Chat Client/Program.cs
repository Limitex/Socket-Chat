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

            var client = new SocketChat.Client();

            IPHostEntry ipHostInfo;
            do
            {
                while (true) 
                { 

                    Console.Write("Server IP : ");
                    try
                    {
                        ipHostInfo = Dns.GetHostEntry(Console.ReadLine());
                        break;
                    }
                    catch
                    {
                        Console.WriteLine("Wrong IP");
                    }
                }

            } while (!client.ConnectServer(ipHostInfo.AddressList[0], 11000));


            Console.Write("User Name : ");
            client.Start(Console.ReadLine());
        }
    }
}
