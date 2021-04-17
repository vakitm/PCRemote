using Newtonsoft.Json;
using SimpleTCP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PCRemote
{
    class TCPServer
    {
        private SimpleTcpServer server;
        private List<TcpClient> connectedClients = new List<TcpClient>();
        private MainForm mainForm;
        public TCPServer(MainForm mainForm)
        {
            this.mainForm = mainForm;            
        }

        /// <summary>
        /// A távvezérléshez használt TCP szervert indítja el
        /// </summary>
        public void startServer()
        {
            //mainForm = MainForm.ActiveForm as MainForm;
            try
            {
                server = new SimpleTcpServer();
                server.Delimiter = 0x13;
                server.DataReceived += Server_DataReceived;
                server.ClientConnected += Server_ClientConnected;
                server.ClientDisconnected += Server_ClientDisconnected;
                server.StringEncoder = Encoding.UTF8;
                server.Start(System.Net.IPAddress.Parse("0.0.0.0"), mainForm.getPort());
                mainForm.changeStatusBar("Server is running",
                                         Color.FromArgb(28, 198, 28));
            }
            catch (System.Net.Sockets.SocketException)
            {
                mainForm.changeStatusBar("Server port:" + mainForm.getPort() + " is being used by another application", Color.FromArgb(255, 136, 0));
            }

        }
        /// <summary>
        /// A távvezérléshez használt TCP szervert állítja le
        /// </summary>
        public void stopServer()
        {
            foreach (TcpClient tc in connectedClients)
                tc.Close();
            server.Stop();
            mainForm.changeStatusBar("Server is stopped", Color.FromArgb(255, 136, 0));
        }
        /// <summary>
        /// Egy kliens lekapcsolódásakor lefutó callback metódus
        /// </summary>
        /// <param name="sender">A küldő objektuma</param>
        /// <param name="e">A paraméterek</param>
        private void Server_ClientDisconnected(object sender, TcpClient e)
        {

            Debug.WriteLine("Disconnected");
            connectedClients.Remove(e);
            mainForm.updateConnectedCLientsText(server.ConnectedClientsCount);
        }
        /// <summary>
        /// Egy kliens kapcsolódásakor lefutó callback metódus
        /// </summary>
        /// <param name="sender">A küldő objektuma</param>
        /// <param name="e">A paraméterek</param>
        private void Server_ClientConnected(object sender, TcpClient e)
        {
            Debug.WriteLine("Connected");
            connectedClients.Add(e);
            mainForm.updateConnectedCLientsText(server.ConnectedClientsCount);
        }
        /// <summary>
        /// Akkor hívódik meg amikor a szervernek valamelyik kliens adatot küld
        /// </summary>
        /// <param name="sender">A küldő objektuma</param>
        /// <param name="e">A paraméterek</param>
        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            Debug.WriteLine("|||" + e.MessageString + "|||");
            string message = e.MessageString;
            try
            {
                if (e.MessageString.Contains("}{"))
                {
                    Debug.WriteLine("Found:" + e.MessageString + " Found");
                    int c = 0;
                    string[] split = message.Split(new string[] { "}{" }, StringSplitOptions.None);
                    foreach (string p in split)
                    {
                        if (c == 0)
                            mainForm.processJson(p + '}');
                        else if (c == split.Length - 1)
                            mainForm.processJson('{' + p);
                        else
                            mainForm.processJson('{' + p + '}');
                        c++;
                    }
                }
                else mainForm.processJson(message);
            }
            catch (JsonReaderException)
            {
            }
        }
        /// <summary>
        /// Visszatér az aktálisan kapcsolódott kliensek számával
        /// </summary>
        /// <returns></returns>
        public int getConnectedClientsCount()
        {
            return server.ConnectedClientsCount;
        }
        /// <summary>
        /// Elküld minden kapcsolódott kliensnek egy üzenetet
        /// </summary>
        /// <param name="text">Az üzenet</param>
        public void Broadcast(string text)
        {
            server.Broadcast(text);
        }
    }
}
