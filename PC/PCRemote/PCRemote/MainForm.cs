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
        
        private TCPServer tcpServer;
        private UDPServer autoDiscoveryServer;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        private int availableRam;
        private long down = 0, up = 0;
        private int x = 0, y = 0;
        private int port = Convert.ToInt32(Properties.Settings.Default.serverport.ToString());
        private int discoveryPort = Convert.ToInt32(Properties.Settings.Default.discoveryport.ToString());

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
            availableRam = Convert.ToInt32(new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024 / 1024);
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

            tcpServer = new TCPServer(this);
            tcpServer.startServer();

            autoDiscoveryServer = new UDPServer(this);
            autoDiscoveryServer.startServer();
        }
        /// <summary>
        /// A telefontól kapott JSON formátumban lévő parancsokat dolgozza fel
        /// </summary>
        /// <param name="message">A JSON kód</param>
        public void processJson(string message)
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
                                tcpServer.stopServer();
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
                                tcpServer.stopServer();
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


        #region ###### METHODS ######
        /// <summary>
        /// A számítógép hangerejét állítja 
        /// </summary>
        /// <param name="Iparam">Hexadecimális kódok</param>
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
        /// <summary>
        /// Bal egér kattintást szimulál
        /// </summary>
        public static void LeftMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        /// <summary>
        /// Bal egér kattintást szimulál
        /// </summary>
        public static void RightMouseClick()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
        /// <summary>
        /// Az állapotsáv szövegét és színét állítja át
        /// </summary>
        /// <param name="text">A kiírandó szöveg</param>
        /// <param name="color">Az beállítandó szín</param>
        public void changeStatusBar(string text, Color color)
        {
            statusBar.BackColor = color;
            statusText.Text = text;
            statusText.BackColor = color;
            if (text.Length > 40)
                statusText.Location = new Point(320, 33);
            else
                statusText.Location = new Point(400, 33);
        }
        /// <summary>
        /// A kapcsolódott kliensek számának értékét állítja át a Formban
        /// </summary>
        /// <param name="value">A kapcsolódott kliensek száma</param>
        public void updateConnectedCLientsText(int value)
        {
            connectedcount.Invoke((MethodInvoker)delegate
            {
                connectedcount.Text = "Connected clients: " + value;
            });
        }
        #endregion


        #region ###### EVENTS ######
        /// <summary>
        /// A form betölésekor hívódik meg, amennyiben az indítás minimalizált módban be van állítva ez a rész minimalizálja a Formot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(Properties.Settings.Default.minimized.ToString())) return;
            BeginInvoke(new MethodInvoker(delegate
            {
                Hide();
            }));
            this.Location = new Point((Screen.PrimaryScreen.Bounds.Size.Width / 2) - (this.Size.Width / 2), (Screen.PrimaryScreen.Bounds.Size.Height / 2) - (this.Size.Height / 2));
        }
        /// <summary>
        /// A tálcaikonon jobbklikkelés után megjelenő menü elemekre való kattintáskor hívódik meg.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// A számítógép adatait küldi el a telefonnak 5 másodpercenként.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                if (tcpServer.getConnectedClientsCount() != 0)
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
                    tcpServer.Broadcast(json + Environment.NewLine);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }
        /// <summary>
        /// A számítógép elalvásakor vagy felébresztésekor hívódik meg.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    break;
                case PowerModes.Resume:
                    tcpServer.startServer();
                    break;
            }

        }
        /// <summary>
        /// Az autómatikus szerverkeresés portját állítja át.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autodiscoveryNumeric_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.discoveryport = Convert.ToInt32(autodiscoveryNumeric.Value);
            Properties.Settings.Default.Save();
            discoveryPort = Convert.ToInt32(autodiscoveryNumeric.Value);
            autoDiscoveryServer.startServer();
        }
        /// <summary>
        /// A szerver portját állítja át
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void portNumeric_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.serverport = Convert.ToInt32(portNumeric.Value);
            Properties.Settings.Default.Save();
            port = Convert.ToInt32(portNumeric.Value);
            tcpServer.stopServer();
            tcpServer.startServer();
        }
        /// <summary>
        /// A minimalizált módban indítást kapcsolja ki vagy be a checkbox-ra kattintva
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void minimize_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.minimized = minimize.Checked;
            Properties.Settings.Default.Save();
        }
        /// <summary>
        /// A rendszerrel való indítást kapcsolja ki vagy be a checkbox-ra kattintva
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// A tálcán lévő ikonra kattintva minimalizálja vagy előtérbe hozza a programot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        /// <summary>
        /// Visszatér az aktuálisan beállított szerver portjával
        /// </summary>
        /// <returns></returns>
        public int getPort()
        {
            return port;
        }
        /// <summary>
        /// Visszatér az aktuálisan beállított autómatikus szerver keresés portjával
        /// </summary>
        /// <returns></returns>
        public int getDiscoveryPort()
        {
            return discoveryPort;
        }
        /// <summary>
        /// Visszatér az aktuális processzor terheléssek %-ban
        /// </summary>
        /// <returns></returns>
        public double getCurrentCpuUsage()
        {
            return cpuCounter.NextValue();
        }
        /// <summary>
        /// Visszatér az aktuális RAM használattal %-ban
        /// </summary>
        /// <returns></returns>
        public double getRAMUsage()
        {
            return ((availableRam - ramCounter.NextValue()) / availableRam) * 100;
        }
        #endregion
    }
    /// <summary>
    /// A számítógép adatainak JSON formátumban való küldéséhez szükséges osztály 
    /// </summary>
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
