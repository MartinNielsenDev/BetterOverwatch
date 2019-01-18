using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace OverwatchTracker
{
    public partial class UpdateNotificationForm : Form
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public int cbSize;
            public IntPtr hwnd;
            public int dwFlags;
            public int uCount;
            public int dwTimeout;
        }
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        const int FLASHW_STOP = 0;
        const int FLASHW_CAPTION = 1;
        const int FLASHW_TRAY = 2;
        const int FLASHW_ALL = 3;
        const int FLASHW_TIMER = 4;
        const int FLASHW_TIMERNOFG = 12;
        public string urlToDownload = String.Empty;
        public UpdateNotificationForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button1.Text = "Updating...";
            if (Server.DownloadUpdate(urlToDownload))
            {
                Server.OpenUpdate();
            }
            else
            {
                button1.Text = "Failed to download";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void UpdateNoficationForm_Load(object sender, EventArgs e)
        {
            this.Deactivate += (s, a) => FlashWindowEx(this.Handle, FLASHW_TRAY);
            this.Activated += (s, a) => FlashWindowEx(this.Handle, FLASHW_STOP);
            SystemSounds.Asterisk.Play();
        }
        private bool FlashWindowEx(IntPtr hWnd, int flags)
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = flags;
            fInfo.uCount = Int32.MaxValue;
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }
    }
}
