using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace FileReceiver
{
    class Program
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        static void Main(string[] args)
        {
            const string TARGETDIR = @"e:\result\";
            const string LINKDIR = @"e:\link\";
            byte[] response = new byte[4];
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

                    string folderPath = TARGETDIR + fileName;
                    string linkFolderPath = LINKDIR + fileName;
                    int index = folderPath.LastIndexOf(@"\");
                    if(index > 0)
                    {
                        folderPath = folderPath.Substring(0, index);
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                    }

                    index = linkFolderPath.LastIndexOf(@"\");
                    if (index > 0)
                    {
                        linkFolderPath = linkFolderPath.Substring(0, index);
                        if (!Directory.Exists(linkFolderPath))
                        {
                            Directory.CreateDirectory(linkFolderPath);
                        }
                    }

                    using (FileStream fs = new FileStream(TARGETDIR + fileName, FileMode.Create))
                    {
                        fs.Write(buffer);
                        fs.Flush();
                        fs.Close();
                        File.Delete(LINKDIR + fileName);
                        bool result = CreateHardLink(LINKDIR + fileName, TARGETDIR + fileName, IntPtr.Zero);
                    }

                    ns.Write(response);
                }
                Console.WriteLine("Client close");
                client.Close();
            }
        }
    }
}
