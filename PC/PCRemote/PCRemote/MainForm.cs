using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using SimpleTCP;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Net;
using System.Text.RegularExpressions;

namespace PCRemote
{
    public partial class MainForm : Form
    {
        Thread autoDiscoveryServerThread;
        SimpleTcpServer server;
        List<TcpClient> connectedClients = new List<TcpClient>();
        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        int availableRam;
        long down = 0, up = 0;
        int x = 0, y = 0;
        int port = Convert.ToInt32(Properties.Settings.Default.serverport.ToString());
        int discoveryPort = Convert.ToInt32(Properties.Settings.Default.discoveryport.ToString());

        #region ###### SystemCalls ######
        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
        [DllImport("user32")]
        public static extern void LockWorkStation();
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy,
        int cButtons, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScan(char ch);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool ExitWindowsEx(uint uFlags, uint dwReason);
        #endregion
        public MainForm()
        {
            InitializeComponent();
            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
            availableRam = Convert.ToInt32(info.TotalPhysicalMemory / 1024 / 1024);
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            notifyIconMain.ContextMenuStrip = notifyIconMain_menu;
            openprogram.Click += new EventHandler(contextmenu_click);
            restart.Click += new EventHandler(contextmenu_click);
            quit.Click += new EventHandler(contextmenu_click);

            portNumeric.Value = Convert.ToInt32(Properties.Settings.Default.serverport.ToString());
            portNumeric.ValueChanged += portNumeric_ValueChanged;
            autodiscoveryNumeric.Value = Convert.ToInt32(Properties.Settings.Default.discoveryport.ToString());
            autodiscoveryNumeric.ValueChanged += autodiscoveryNumeric_ValueChanged;

            SystemEvents.PowerModeChanged += OnPowerModeChanged;

            minimize.Checked = Convert.ToBoolean(Properties.Settings.Default.minimized.ToString());
            autostart.Checked = Convert.ToBoolean(Properties.Settings.Default.autostart.ToString());

            startServer();
            startAutoDiscoveryServer();
        }
        #region ###### SERVER ######
        private void startAutoDiscoveryServer()
        {
            if (autoDiscoveryServerThread != null) autoDiscoveryServerThread.Abort();
            autoDiscoveryServerThread = new Thread(() =>
            {
                  try
                  {
                    Thread.CurrentThread.IsBackground = true;
                    var Server = new UdpClient(discoveryPort);
                    while (true)
                    {
                        var ClientEp = new IPEndPoint(IPAddress.Any, 0);
                        var ClientRequestData = Server.Receive(ref ClientEp);
                        var ClientRequest = Encoding.ASCII.GetString(ClientRequestData);

                        Console.WriteLine("Recived {0} from {1}, sending response", ClientRequest, ClientEp.Address.ToString());
                        var ResponseData = Encoding.ASCII.GetBytes("PCREMOTE_DISCOVER_RESPONSE:" + port);
                        Server.Send(ResponseData, ResponseData.Length, ClientEp);
                    }
                 }
                 catch(System.Net.Sockets.SocketException)
                {
                }
            });
            autoDiscoveryServerThread.Start();
        }
        private void stopServer()
        {
            foreach (TcpClient tc in connectedClients)
                tc.Close();
            server.Stop();
            changeStatusBar("Server is stopped", Color.FromArgb(255, 136, 0));
        }

        private void startServer()
        {
            try
            {
                server = new SimpleTcpServer();
                server.Delimiter = 0x13;
                server.DataReceived += Server_DataReceived;
                server.ClientConnected += Server_ClientConnected;
                server.ClientDisconnected += Server_ClientDisconnected;
                server.StringEncoder = Encoding.UTF8;
                server.Start(System.Net.IPAddress.Parse("0.0.0.0"), port);
                changeStatusBar("Server is running", Color.FromArgb(28, 198, 28));
            }
            catch(System.Net.Sockets.SocketException)
            {
                changeStatusBar("Server port:" + port + " is being used by another application", Color.FromArgb(255, 136, 0));
            }
            
        }
        private void Server_ClientDisconnected(object sender, TcpClient e)
        {

            Debug.WriteLine("Disconnected");
            connectedClients.Remove(e);
            connectedcount.Invoke((MethodInvoker)delegate
            {
                connectedcount.Text = "Connected clients: " + server.ConnectedClientsCount;
            });
        }

        private void Server_ClientConnected(object sender, TcpClient e)
        {
            Debug.WriteLine("Connected");
                connectedClients.Add(e);
                connectedcount.Invoke((MethodInvoker)delegate
                {
                    connectedcount.Text = "Connected clients: " + server.ConnectedClientsCount;
                });
        }

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
                    foreach (string asd in split)
                    {
                        if (c == 0)
                            processJson(asd + '}');
                        else if (c == split.Length - 1)
                            processJson('{' + asd);
                        else
                            processJson('{' + asd + '}');
                        c++;
                    }
                }
                else processJson(message);
            }
            catch (JsonReaderException)
            {
            }
        }
        private void processJson(string message)
        {
            try
            {
                Dictionary<string, string> json = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
                switch (json["a"])
                {
                    case "d":

                        x = Convert.ToInt32(json["x"]);
                        y = Convert.ToInt32(json["y"]);
                        break;
                    case "k":
                        if (json["k"] == "bs")
                            keybd_event((byte)0x08, 0x9e, 0, 0);
                        else if (json["k"] == "en")
                            keybd_event((byte)System.Windows.Forms.Keys.Enter, 0x45, 0, 0);
                        else if (json["k"] == "vu")
                            VolumeControl(APPCOMMAND_VOLUME_UP);
                        else if (json["k"] == "vd")
                            VolumeControl(APPCOMMAND_VOLUME_DOWN);
                        else
                        {
                            byte keybd;
                            bool isCapital;
                            if (int.TryParse(json["k"], out _))
                            {
                                char key = Convert.ToChar(Convert.ToInt32(json["k"]));
                                Debug.WriteLine(Convert.ToString(key));
                                Debug.WriteLine(VkKeyScan(key));
                                if (json["cpt"] == "1")
                                {
                                    if (VkKeyScan(key) > 255)
                                    {
                                        SendKeys.SendWait(Regex.Replace(key + "", "[+^%~()]", "{$0}") + "");
                                        Debug.WriteLine("SendKeys");
                                        return;
                                    }
                                    else
                                        keybd = Convert.ToByte(key);
                                    isCapital = true;
                                }
                                else
                                {
                                    if (VkKeyScan(key) > 255)
                                    {
                                        SendKeys.SendWait(Regex.Replace(key + "", "[+^%~()]", "{$0}") + "");
                                        Debug.WriteLine("SendKeys");
                                        return;
                                    }
                                    else
                                        keybd = Convert.ToByte(VkKeyScan(key));
                                    isCapital = false;
                                }
                            }
                            else
                            {
                                if (Char.IsUpper(Convert.ToChar(json["k"])))
                                    isCapital = true;
                                else
                                    isCapital = false;
                                keybd = Convert.ToByte(VkKeyScan(Char.ToLower(Convert.ToChar(json["k"]))));
                            }
                            if (isCapital)
                                keybd_event(16, 0, 0, 0);

                            keybd_event(keybd, 0x9e, 0, 0);

                            if (isCapital)
                                keybd_event(16, 0, 0x0002, 0);
                        }
                        break;
                    case "m":
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            Invoke(new Action(delegate
                            {

                                int xmove, ymove;
                                if (x > Convert.ToInt32(json["x"]))
                                    xmove = x - Convert.ToInt32(json["x"]);
                                else if (x < Convert.ToInt32(json["x"]))
                                    xmove = x - Convert.ToInt32(json["x"]);
                                else xmove = 0;
                                if (y > Convert.ToInt32(json["y"]))
                                    ymove = y - Convert.ToInt32(json["y"]);
                                else if (y < Convert.ToInt32(json["y"]))
                                    ymove = y - Convert.ToInt32(json["y"]);
                                else ymove = 0;
                                this.Cursor = new Cursor(Cursor.Current.Handle);
                                Cursor.Position = new Point(Cursor.Position.X - xmove, Cursor.Position.Y - ymove);
                                x = Convert.ToInt32(json["x"]);
                                y = Convert.ToInt32(json["y"]);
                            }));
                        });
                        break;
                    case "lc":
                        LeftMouseClick();
                        break;
                    case "rc":
                        RightMouseClick();
                        break;
                    case "sd":
                        Debug.WriteLine("Shut down");
                        Process.Start("shutdown", "/s /t 0");
                        break;
                    case "rs":
                        Debug.WriteLine("Restart");
                        Process.Start("shutdown.exe", "/r /t 0");
                        break;
                    case "sl":
                        Debug.WriteLine("Sleep");
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            Invoke(new Action(delegate
                            {
                                stopServer();
                                SetSuspendState(false, true, true);
                            }));
                        });
                        break;
                    case "hb":
                        Debug.WriteLine("Hibernate");
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            Invoke(new Action(delegate
                            {
                                stopServer();
                                SetSuspendState(true, true, true);
                            }));
                        });
                        break;
                    case "lo":
                        Debug.WriteLine("Log off");
                        ExitWindowsEx(0, 0);
                        break;
                    case "lk":
                        LockWorkStation();
                        Debug.WriteLine("Lock");
                        break;
                    case "vu":
                        VolumeControl(APPCOMMAND_VOLUME_UP);
                        break;
                    case "vd":
                        VolumeControl(APPCOMMAND_VOLUME_DOWN);
                        break;
                    case "vm":
                        VolumeControl(APPCOMMAND_VOLUME_MUTE);
                        break;
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }
        #endregion


        #region ###### METHODS ######
        public void VolumeControl(int Iparam)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                Invoke(new Action(delegate
                {
                    SendMessageW(this.Handle, WM_APPCOMMAND, this.Handle, (IntPtr)Iparam);
                }));
            });
            
        }
        public static void LeftMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        public static void RightMouseClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
        private void changeStatusBar(string text, Color color)
        {
            statusBar.BackColor = color;
            statusText.Text = text;
            statusText.BackColor = color;
            if (text.Length > 40)
                statusText.Location = new Point(320, 33);
            else
                statusText.Location = new Point(400, 33);
        }
        #endregion


        #region ###### EVENTS ######
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(Properties.Settings.Default.minimized.ToString())) return;
            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
            }));
            this.Location = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - (this.Size.Width / 2), (Screen.PrimaryScreen.Bounds.Size.Height / 2) - (this.Size.Height / 2));
        }
        private void contextmenu_click(object sender, EventArgs e)
        {
            ToolStripMenuItem clickedItem = (ToolStripMenuItem)sender;
            switch (clickedItem.Name)
            {
                case "openprogram":
                    this.Visible = true;
                    break;
                case "restart":
                    System.Diagnostics.Process.Start(Application.ExecutablePath);
                    Environment.Exit(1);
                    break;
                case "quit":
                    Environment.Exit(1);
                    break;
            }
        }
        private void statusTimer_Tick(object sender, EventArgs e)
        {
            int ping = 0;
            long upload = 0;
            long download = 0;
            bool networkstatus = true;
            try
            {
                NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();
                NetworkInterface ni = networks.First(x => x.NetworkInterfaceType != NetworkInterfaceType.Loopback
                   && x.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                   && x.OperationalStatus == OperationalStatus.Up
                   && x.Name.StartsWith("vEthernet") == false);

                Ping myPing = new Ping();

                PingReply reply = myPing.Send("google.com", 5000);
                if (reply != null)
                {
                    ping = Convert.ToInt32(reply.RoundtripTime);
                }


                download = (ni.GetIPv4Statistics().BytesReceived - down) / 1024 / 5; down = ni.GetIPv4Statistics().BytesReceived;
                upload = (ni.GetIPv4Statistics().BytesSent - up) / 1024 / 5; up = ni.GetIPv4Statistics().BytesSent;
            }
            catch { networkstatus = false; }
            try
            {
                if (server.ConnectedClientsCount != 0)
                {
                    CheckIn checkin = new CheckIn
                    {
                        task = "checkin",
                        cpu = Convert.ToInt32(getCurrentCpuUsage()),
                        ram = Convert.ToInt32(getRAMUsage()),
                        up = Convert.ToInt32(upload),
                        down = Convert.ToInt32(download),
                        ping = ping,
                        network = networkstatus
                    };
                    string json = JsonConvert.SerializeObject(checkin, Formatting.None);
                    server.Broadcast(json + Environment.NewLine);
                    //Debug.WriteLine(json);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }
        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    break;
                case PowerModes.Resume:
                    startServer();
                    break;
            }

        }
        private void autodiscoveryNumeric_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.discoveryport = Convert.ToInt32(autodiscoveryNumeric.Value);
            Properties.Settings.Default.Save();
            discoveryPort = Convert.ToInt32(autodiscoveryNumeric.Value);
            startAutoDiscoveryServer();
        }

        private void portNumeric_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.serverport = Convert.ToInt32(portNumeric.Value);
            Properties.Settings.Default.Save();
            port = Convert.ToInt32(portNumeric.Value);
            stopServer();
            startServer();
        }
        private void minimize_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.minimized = minimize.Checked;
            Properties.Settings.Default.Save();
        }

        private void autostart_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.autostart = autostart.Checked;
            Properties.Settings.Default.Save();
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (autostart.Checked)
                rk.SetValue("PCRemote Server", Application.ExecutablePath);
            else
                rk.DeleteValue("PCRemote Server", false);
        }
        private void notifyIconMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Visible = !this.Visible;
            }
        }
        #endregion


        #region ###### ToplabelEvents ######
        private void topbar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        private void toplabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, 0xA1, 0x2, 0);
            }
        }

        private void exit_icon_MouseHover(object sender, EventArgs e)
        {
            exit_icon.BackColor = Color.FromArgb(255, 0, 0);
        }

        private void exit_icon_MouseLeave(object sender, EventArgs e)
        {
            exit_icon.BackColor = Color.FromArgb(0, 102, 204);
        }

        private void exit_icon_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
        #endregion


        #region ###### GETTERS ######
        private string getLocalIP()
        {
            String strHostName = Dns.GetHostName();
            IPHostEntry ipHostEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] address = ipHostEntry.AddressList;
            return address[4].ToString();
        }
        public double getCurrentCpuUsage()
        {
            return cpuCounter.NextValue();
        }

        public double getRAMUsage()
        {
            return ((availableRam - ramCounter.NextValue()) / availableRam) * 100;
        }
        #endregion
    }
    class CheckIn
    {
        public string task { get; set; }
        public int cpu { get; set; }
        public int ram { get; set; }
        public int up { get; set; }
        public int down { get; set; }
        public int ping { get; set; }
        public bool network { get; set; }
    }

}
