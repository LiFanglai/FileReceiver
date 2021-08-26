using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FileReceiver
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] response = new byte[256];
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 9527);
            Console.WriteLine("TcpListener start");
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Accept client {0}", client.Client.LocalEndPoint.ToString());
                NetworkStream ns = client.GetStream();
                StreamReader reader = new StreamReader(ns);
                int fileCount = Convert.ToInt32(reader.ReadLine());
                for (int i = 0; i < fileCount; i++)
                {
                    int fileSize = Convert.ToInt32(reader.ReadLine());
                    Console.WriteLine(fileSize);
                    string fileName = reader.ReadLine();
                    Console.WriteLine(fileName);
                    ns.Write(response);
                    byte[] buffer = new byte[fileSize];
                    int receivedSize = 0;
                    int readSize = 1024;

                    while (receivedSize < fileSize)
                    {
                        readSize = fileSize - receivedSize < readSize ? fileSize - receivedSize : readSize;
                        int read = ns.Read(buffer, receivedSize, readSize);
                        receivedSize += read;
                    }

                    Console.WriteLine("FileStream write {0}", fileName);
                    using (FileStream fs = new FileStream(@"e:\" + fileName, FileMode.Create))
                    {
                        fs.Write(buffer);
                        fs.Flush();
                        fs.Close();
                    }

                    ns.Write(response);
                }
                Console.WriteLine("Client close");
                client.Close();
            }
        }
    }
}
