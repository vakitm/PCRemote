namespace PCRemote
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIconMain = new System.Windows.Forms.NotifyIcon(this.components);
            this.toplabel = new System.Windows.Forms.Label();
            this.topbar = new System.Windows.Forms.Label();
            this.notifyIconMain_menu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openprogram = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.connect = new System.Windows.Forms.ToolStripMenuItem();
            this.restart = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.quit = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.exit_icon = new System.Windows.Forms.PictureBox();
            this.statusTimer = new System.Windows.Forms.Timer(this.components);
            this.connectedcount = new System.Windows.Forms.Label();
            this.leftbar = new System.Windows.Forms.Label();
            this.notifyIconMain_menu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.exit_icon)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIconMain
            // 
            this.notifyIconMain.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIconMain.Icon")));
            this.notifyIconMain.Text = "PCRemote Server";
            this.notifyIconMain.Visible = true;
            this.notifyIconMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIconMain_MouseClick);
            // 
            // toplabel
            // 
            this.toplabel.AutoSize = true;
            this.toplabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.toplabel.Font = new System.Drawing.Font("Candara", 11.25F);
            this.toplabel.ForeColor = System.Drawing.Color.White;
            this.toplabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.toplabel.Location = new System.Drawing.Point(9, 6);
            this.toplabel.Name = "toplabel";
            this.toplabel.Size = new System.Drawing.Size(118, 18);
            this.toplabel.TabIndex = 65;
            this.toplabel.Text = "PCRemote Server";
            this.toplabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.toplabel_MouseDown);
            // 
            // topbar
            // 
            this.topbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.topbar.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.topbar.Location = new System.Drawing.Point(0, 0);
            this.topbar.Name = "topbar";
            this.topbar.Size = new System.Drawing.Size(800, 30);
            this.topbar.TabIndex = 64;
            this.topbar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.topbar_MouseDown);
            // 
            // notifyIconMain_menu
            // 
            this.notifyIconMain_menu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openprogram,
            this.toolStripSeparator1,
            this.connect,
            this.restart,
            this.toolStripSeparator2,
            this.quit});
            this.notifyIconMain_menu.Name = "notifyicon_menu";
            this.notifyIconMain_menu.Size = new System.Drawing.Size(198, 104);
            // 
            // openprogram
            // 
            this.openprogram.Image = ((System.Drawing.Image)(resources.GetObject("openprogram.Image")));
            this.openprogram.Name = "openprogram";
            this.openprogram.Size = new System.Drawing.Size(197, 22);
            this.openprogram.Text = "Open PCRemote Server";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(194, 6);
            // 
            // connect
            // 
            this.connect.Name = "connect";
            this.connect.Size = new System.Drawing.Size(197, 22);
            this.connect.Text = "Connected";
            // 
            // restart
            // 
            this.restart.Name = "restart";
            this.restart.Size = new System.Drawing.Size(197, 22);
            this.restart.Text = "Restart";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(194, 6);
            // 
            // quit
            // 
            this.quit.Name = "quit";
            this.quit.Size = new System.Drawing.Size(197, 22);
            this.quit.Text = "Quit";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(180)))));
            this.pictureBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.pictureBox1.Image = global::PCRemote.Properties.Resources.logo;
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(12, 33);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 68;
            this.pictureBox1.TabStop = false;
            // 
            // exit_icon
            // 
            this.exit_icon.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(102)))), ((int)(((byte)(204)))));
            this.exit_icon.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.exit_icon.ErrorImage = ((System.Drawing.Image)(resources.GetObject("exit_icon.ErrorImage")));
            this.exit_icon.Image = ((System.Drawing.Image)(resources.GetObject("exit_icon.Image")));
            this.exit_icon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.exit_icon.Location = new System.Drawing.Point(775, 5);
            this.exit_icon.Name = "exit_icon";
            this.exit_icon.Size = new System.Drawing.Size(19, 19);
            this.exit_icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.exit_icon.TabIndex = 66;
            this.exit_icon.TabStop = false;
            this.exit_icon.Click += new System.EventHandler(this.exit_icon_Click);
            this.exit_icon.MouseLeave += new System.EventHandler(this.exit_icon_MouseLeave);
            this.exit_icon.MouseHover += new System.EventHandler(this.exit_icon_MouseHover);
            // 
            // statusTimer
            // 
            this.statusTimer.Enabled = true;
            this.statusTimer.Interval = 5000;
            this.statusTimer.Tick += new System.EventHandler(this.statusTimer_Tick);
            // 
            // connectedcount
            // 
            this.connectedcount.AutoSize = true;
            this.connectedcount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(180)))));
            this.connectedcount.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.connectedcount.ForeColor = System.Drawing.Color.White;
            this.connectedcount.Location = new System.Drawing.Point(9, 145);
            this.connectedcount.Name = "connectedcount";
            this.connectedcount.Size = new System.Drawing.Size(124, 17);
            this.connectedcount.TabIndex = 69;
            this.connectedcount.Text = "Connected clients: 0";
            this.connectedcount.Click += new System.EventHandler(this.connectedcount_Click);
            // 
            // leftbar
            // 
            this.leftbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(180)))));
            this.leftbar.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.leftbar.Location = new System.Drawing.Point(0, 30);
            this.leftbar.Name = "leftbar";
            this.leftbar.Size = new System.Drawing.Size(144, 425);
            this.leftbar.TabIndex = 70;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(46)))), ((int)(((byte)(59)))));
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.connectedcount);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.exit_icon);
            this.Controls.Add(this.toplabel);
            this.Controls.Add(this.topbar);
            this.Controls.Add(this.leftbar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PCRemote Server";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.notifyIconMain_menu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.exit_icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIconMain;
        private System.Windows.Forms.PictureBox exit_icon;
        private System.Windows.Forms.Label toplabel;
        private System.Windows.Forms.Label topbar;
        private System.Windows.Forms.ContextMenuStrip notifyIconMain_menu;
        private System.Windows.Forms.ToolStripMenuItem openprogram;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem connect;
        private System.Windows.Forms.ToolStripMenuItem restart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem quit;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer statusTimer;
        private System.Windows.Forms.Label connectedcount;
        private System.Windows.Forms.Label leftbar;
    }
}

