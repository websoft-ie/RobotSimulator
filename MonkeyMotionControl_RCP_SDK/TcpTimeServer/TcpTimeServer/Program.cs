using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpTimeServer
{
    class Program
    {
        private static void DataReceived(IAsyncResult ar)
        {
            UdpClient c = (UdpClient)ar.AsyncState;
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            Byte[] receivedBytes = c.EndReceive(ar, ref receivedIpEndPoint);
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.ff tt") + " (" + receivedBytes.Length + " bytes)");
            Console.WriteLine("UDP: " + Encoding.ASCII.GetString(receivedBytes));

            //byte[] buffer;

            //for (; ; )
            //{
            //    remoteEP = null;
            //    buffer = server.Receive(ref remoteEP);

            //    if (buffer != null && buffer.Length > 0)
            //    {
            //        Console.WriteLine("UDP: " + Encoding.ASCII.GetString(buffer));
            //    }
            //}

            c.BeginReceive(DataReceived, ar.AsyncState);
        }
        static void Main(string[] args)
        {
            TcpListener tcpServer = null;
            UdpClient udpServer = null;
            int port = 59567;

            Console.WriteLine(string.Format("Starting TCP and UDP servers on port {0}...", port));

            try
            {
                udpServer = new UdpClient(port);
                udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpServer.BeginReceive(DataReceived, udpServer);

                tcpServer = new TcpListener(IPAddress.Any, port);

                var udpThread = new Thread(new ParameterizedThreadStart(UDPServerProc));
                udpThread.IsBackground = true;
                udpThread.Name = "UDP server thread";
                //udpThread.Start(udpServer);

                var tcpThread = new Thread(new ParameterizedThreadStart(TCPServerProc));
                tcpThread.IsBackground = true;
                tcpThread.Name = "TCP server thread";
                tcpThread.Start(tcpServer);

                Console.WriteLine("Press <ENTER> to stop the servers.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main exception: " + ex);
            }
            finally
            {
                if (udpServer != null)
                    udpServer.Close();

                if (tcpServer != null)
                    tcpServer.Stop();
            }

            Console.WriteLine("Press <ENTER> to exit.");
            Console.ReadLine();
        }

        private static void UDPServerProc(object arg)
        {
            Console.WriteLine("UDP server thread started");

            try
            {
                UdpClient server = (UdpClient)arg;
                IPEndPoint remoteEP;
                byte[] buffer;

                for (; ; )
                {
                    remoteEP = null;
                    buffer = server.Receive(ref remoteEP);

                    if (buffer != null && buffer.Length > 0)
                    {
                        Console.WriteLine("UDP: " + Encoding.ASCII.GetString(buffer));
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10004) // unexpected
                    Console.WriteLine("UDPServerProc exception: " + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UDPServerProc exception: " + ex);
            }

            Console.WriteLine("UDP server thread finished");
        }

        private static void TCPServerProc(object arg)
        {
            Console.WriteLine("TCP server thread started");

            try
            {
                TcpListener server = (TcpListener)arg;
                byte[] buffer = new byte[2048];
                int count;

                server.Start();

                for (; ; )
                {
                    TcpClient client = server.AcceptTcpClient();

                    using (var stream = client.GetStream())
                    {
                        while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            Console.WriteLine("TCP: " + Encoding.ASCII.GetString(buffer, 0, count));
                        }
                    }
                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10004) // unexpected
                    Console.WriteLine("TCPServerProc exception: " + ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TCPServerProc exception: " + ex);
            }

            Console.WriteLine("TCP server thread finished");
        }
    }
}