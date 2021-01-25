using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rssdp;
using SimpleTCP;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.Devices;

namespace PCRemote
{
    public partial class MainForm : Form
    {
        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy,
        int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        SimpleTcpServer server;
        int availableRam;
        long down, up;
        int x, y;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short VkKeyScan(char ch);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        public MainForm()
        {
            InitializeComponent();
            cpuCounter = new PerformanceCounter();
            up = 0; down = 0;
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";
            var info = new Microsoft.VisualBasic.Devices.ComputerInfo();
            availableRam = Convert.ToInt32(info.TotalPhysicalMemory / 1024 / 1024);
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 2000);
            notifyIconMain.ContextMenuStrip = notifyIconMain_menu;
            openprogram.Click += new EventHandler(contextmenu_click);
            restart.Click += new EventHandler(contextmenu_click);
            quit.Click += new EventHandler(contextmenu_click);

            server = new SimpleTcpServer();
            server.Delimiter = 0x13;
            server.DataReceived += Server_DataReceived;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;

            server.StringEncoder = Encoding.UTF8;
            server.Start(System.Net.IPAddress.Parse("0.0.0.0"), 1337);
            x = 0; y = 0;
        }
        public void PublishDevice()
        {
            // As this is a sample, we are only setting the minimum required properties.
            var deviceDefinition = new SsdpRootDevice()
            {
                CacheLifetime = TimeSpan.FromMinutes(30), //How long SSDP clients can cache this info.
                Location = new Uri("http://mydevice/descriptiondocument.xml"), // Must point to the URL that serves your devices UPnP description document. 
                DeviceTypeNamespace = "my-namespace",
                DeviceType = "MyCustomDevice",
                FriendlyName = "Custom Device 1",
                Manufacturer = "Me",
                ModelName = "MyCustomDevice",
                Uuid = "asd" // This must be a globally unique value that survives reboots etc. Get from storage or embedded hardware etc.
            };
        }
        private void Server_ClientDisconnected(object sender, TcpClient e)
        {

            Debug.WriteLine("Disconnected");
            connectedcount.Invoke((MethodInvoker)delegate
            {
                connectedcount.Text = "Connected clients: " + server.ConnectedClientsCount;
            });
        }

        private void Server_ClientConnected(object sender, TcpClient e)
        {
            Debug.WriteLine("Connected");
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
            //PublishDevice();
        }
        private void processJson(string message)
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
                        char key = Convert.ToChar(Convert.ToInt32(json["k"]));
                        if (VkKeyScan(key) > 255) return;
                        Debug.WriteLine(Convert.ToString(key));
                        Debug.WriteLine(VkKeyScan(key));
                        keybd_event(Convert.ToByte(VkKeyScan(key)), 0x9e, 0, 0);
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
                    break;
                case "rs":
                    Debug.WriteLine("Restart");
                    break;
                case "sl":
                    Debug.WriteLine("Sleep");
                    break;
                case "hb":
                    Debug.WriteLine("Hibernate");
                    break;
                case "lo":
                    Debug.WriteLine("Log off");
                    break;
                case "lk":
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
        private void MainForm_Load(object sender, EventArgs e)
        {
            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
            }));
            this.Location = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - (this.Size.Width / 2), (Screen.PrimaryScreen.Bounds.Size.Height / 2) - (this.Size.Height / 2));
        }

        private void notifyIconMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Visible = !this.Visible;
            }
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

        #region ToplabelEvents
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

        private void button1_Click(object sender, EventArgs e)
        {
            server.Broadcast("werikci" + Environment.NewLine);
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
                //NetworkInterface ni = networks[0];


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

        private void connectedcount_Click(object sender, EventArgs e)
        {

        }

        public double getCurrentCpuUsage()
        {
            return cpuCounter.NextValue();
        }

        public double getRAMUsage()
        {
            return ((availableRam - ramCounter.NextValue()) / availableRam) * 100;
        }
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
