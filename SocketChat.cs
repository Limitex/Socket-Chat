using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketComunication
{
    public struct SendData
    {
        public string ID;
        public string DATA;
    }

    public class SocketChat
    {
        public class Server
        {
            public static bool ServerRunning { get; set; }

            private Socket MainListener;

            private static List<IndividualSocket> individualSocket = new List<IndividualSocket>();

            public Server(IPAddress ipAddress, int port)
            {
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
                MainListener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                MainListener.Bind(localEndPoint);
            }

            public void Start()
            {
                ServerRunning = true;
                MainListener.Listen(10);
                while (ServerRunning)
                {
                    Console.WriteLine("Listening...");
                    individualSocket.Add(new IndividualSocket(MainListener.Accept()));
                }
            }

            private static void SocketDelete()
            {
                for (int i = 0; i < individualSocket.Count; i++)
                {
                    if (!individualSocket[i].IsRunning) individualSocket.RemoveAt(i--);
                }
            }

            private class IndividualSocket
            {
                public bool IsRunning { get; set; }
                private Socket socket;
                private Thread thread;
                private SendAndReceive sendAndReceive;
                public IndividualSocket(Socket s)
                {
                    socket = s;
                    sendAndReceive = new SendAndReceive(socket);
                    IsRunning = true;
                    thread = new Thread(Run);
                    thread.Start();
                    Console.WriteLine("Client was connected.");
                }
                private void Run()
                {
                    var startRecaive = sendAndReceive.Receive();
                    var start = new SendData[2];
                    start[0].ID = "usn";
                    start[0].DATA = "Server";
                    start[1].ID = "msg";
                    start[1].DATA = "Welcome " + startRecaive.FindSendData("usn")[0];
                    sendAndReceive.Send(start);

                    while (IsRunning)
                    {
                        SendData[] receiveData = sendAndReceive.Receive();
                        if (receiveData == null)
                        {
                            Disconected();
                            return;
                        }
                        foreach (var cliants in individualSocket)
                        {
                            cliants.sendAndReceive.Send(receiveData);
                        }
                    }
                }
                private void Disconected()
                {
                    IsRunning = false;
                    Console.WriteLine("Client was disconnected.");
                    SocketDelete();
                }
            }
        }

        public class Client
        {
            public static bool ClientRunning { get; set; }

            private Socket MainListener;

            private SendAndReceive sendAndReceive;

            public bool ConnectServer(IPAddress ipAddress, int port)
            {
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

                MainListener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    MainListener.Connect(localEndPoint);
                }
                catch
                {
                    Console.WriteLine("Server is not found");
                    return false;
                }
                sendAndReceive = new SendAndReceive(MainListener);
                Console.WriteLine("Conected.");
                return true;
            }

            public void Start(string userName)
            {
                var start = new SendData[1];
                start[0].ID = "usn";
                start[0].DATA = userName;
                sendAndReceive.Send(start);

                ClientRunning = true;
                var listen = new Thread(() => {
                    while (ClientRunning)
                    {
                        var data = sendAndReceive.Receive();
                        var username = data.FindSendData("usn")[0];
                        foreach (var msg in data.FindSendData("msg"))
                        {
                            Console.WriteLine("[" + username + "] : " + msg);
                        }

                    }
                });
                listen.Start();

                while (ClientRunning)
                {
                    var input = Console.ReadLine();
                    var sendData = new SendData[2];
                    sendData[0] = SendAndReceive.SetData("usn", userName);
                    sendData[1] = SendAndReceive.SetData("msg", input);

                    sendAndReceive.Send(sendData);
                }

                MainListener.Shutdown(SocketShutdown.Both);
                MainListener.Close();
            }
        }

        private class SendAndReceive
        {
            private Socket socket;

            public SendAndReceive(Socket s)
            {
                socket = s;
            }
            public static SendData SetData(string id, string data)
            {
                var sd = new SendData();
                sd.ID = id;
                sd.DATA = data;
                return sd;
            }

            public void Send(SendData[] sd)
            {
                string send = "{";
                for (int i = 0; i < sd.Length; i++)
                    send += sd[i].ID + "=\"" + sd[i].DATA + "\",";
                send = send.TrimEnd(',') + "}";
                socket.Send(Encoding.UTF8.GetBytes(send));
            }

            public SendData[] Receive()
            {
                var bytes = new byte[1024];
                int receive;
                try
                {
                    receive = socket.Receive(bytes);
                }
                catch
                {
                    return null;
                }
                var RawData = Encoding.UTF8.GetString(bytes, 0, receive);
                var Data = RawData.TrimStart('{').TrimEnd('}').Split(',');

                var sd = new SendData[Data.Length];
                for (int i = 0; i < Data.Length; i++)
                {
                    sd[i] = new SendData();
                    var d = Data[i].Split('=');
                    sd[i].ID = d[0];
                    sd[i].DATA = d[1].TrimStart('\"').TrimEnd('\"');
                }
                return sd;
            }
        }


    }
    static class Expansion
    {
        public static string[] FindSendData(this SendData[] sd, string id)
        {
            var found = new List<string>();
            for (int i = 0; i < sd.Length; i++)
            {
                if (sd[i].ID == id)
                {
                    found.Add(sd[i].DATA);
                }
            }
            if (found.Count == 0)
            {
                return new string[1];
            }
            return found.ToArray();
        }
    }
}
