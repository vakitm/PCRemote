using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCRemote
{
    class UDPServer
    {
        private MainForm mainForm;
        private Thread autoDiscoveryServerThread;

        public UDPServer(MainForm mainForm)
        {
            this.mainForm = mainForm;
        }

        public MainForm MainForm
        {
            get => default;
            set
            {
            }
        }

        ////A https://stackoverflow.com/questions/22852781/how-to-do-network-discovery-using-udp-broadcast linkről másolt programkód részletet használtam alapul az alább található részhez.
        /// <summary>
        /// Elindítja az autómaikus szerver megtaláláshoz használt UDP szervert
        /// </summary>
        public void startServer()
        {
            if (autoDiscoveryServerThread != null) autoDiscoveryServerThread.Abort();
            autoDiscoveryServerThread = new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.IsBackground = true;
                    var Server = new UdpClient(mainForm.getDiscoveryPort());
                    while (true)
                    {
                        var ClientEp = new IPEndPoint(IPAddress.Any, 0);
                        var ClientRequestData = Server.Receive(ref ClientEp);
                        var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

                        Console.WriteLine("Recived {0} from {1}, sending response", ClientRequest, ClientEp.Address.ToString());
                        var ResponseData = Encoding.ASCII.GetBytes("PCREMOTE_DISCOVER_RESPONSE:" + mainForm.getPort());
                        Server.Send(ResponseData, ResponseData.Length, ClientEp);
                    }
                }
                catch (System.Net.Sockets.SocketException)
                {
                }
            });
            autoDiscoveryServerThread.Start();
        }
        /// <summary>
        /// Leállítja az autómaikus szerver megtaláláshoz használt UDP szervert
        /// </summary>
        public void stopServer()
        {
            if (autoDiscoveryServerThread != null) autoDiscoveryServerThread.Abort();
        }
    }
}
