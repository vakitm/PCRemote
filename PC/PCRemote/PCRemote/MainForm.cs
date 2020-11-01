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
namespace PCRemote
{
    public partial class MainForm : Form
    {
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        SimpleTcpServer server;
        private SsdpDevicePublisher _Publisher;

        public MainForm()
        {
            InitializeComponent();
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
        }

        private void Server_ClientConnected(object sender, TcpClient e)
        {
            Debug.WriteLine("Connected");
        }

        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            Debug.WriteLine(e.MessageString);
            e.Reply(e.MessageString);
            PublishDevice();
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
            server.Broadcast("werikci"+ Environment.NewLine);
        }
    }
}
