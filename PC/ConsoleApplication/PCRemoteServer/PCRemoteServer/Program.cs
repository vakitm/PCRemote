using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRemoteServer
{
    class Program
    {
        static void Main(string[] args)
        {
            string myFolder = @"C:\folderpath\to\serve";
            SimpleHTTPServer myServer;

            //create server with auto assigned port
            myServer = new SimpleHTTPServer(myFolder);


            //Creating server with specified port
            myServer = new SimpleHTTPServer(myFolder, 8084);


            //Now it is running:
            Console.WriteLine("Server is running on this port: " + myServer.Port.ToString());

        }
    }
}
