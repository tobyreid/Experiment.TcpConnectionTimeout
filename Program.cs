using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Experiment.TcpConnectionTimeout
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply a host and port in the following format:");
                Console.WriteLine();
                Console.WriteLine("\"{0} 23.102.49.51 443\"", System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name);
            }
            else
            {
                int port;
                if (int.TryParse(args[1], out port))
                {
                    TestConnection(args[0], port);
                }
                else
                {
                    Console.WriteLine("Invalid port");
                }

            }
            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadLine();
        }

        static void TestConnection(string ipAddress, int port)
        {
            using (var tcpClient = new TcpClient())
            {
                var result = tcpClient.BeginConnect(ipAddress, port, null, null);
                var timeoutHandler = result.AsyncWaitHandle;
                var sw = new Stopwatch();
                sw.Start();
                try
                {
                    var top = Console.WindowTop;
                    Console.WriteLine("Testing TCP timeout for {0} : {1}",ipAddress,port);
                    while (true)
                    {

                        Thread.Sleep(250);
                        Console.SetCursorPosition(0, top + 1);
                        Console.WriteLine("Elapsed : {0}",sw.Elapsed);
                        // Detect if client disconnected
                        if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];
                            if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                // Client disconnected
                                return;
                            }
                        }
                        
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    sw.Stop();
                    Console.WriteLine("Timeout after : {0}", sw.Elapsed);
                    timeoutHandler.Close();
                }
            }
        }
    }
}
