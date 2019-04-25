using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BetterOverwatch.Forms
{
    public partial class UpdateNotificationForm : Form
    {
        public string downloadUrl = string.Empty;
        public int downloadSize = 0;
        private int downloadProgress;

        public UpdateNotificationForm()
        {
            InitializeComponent();
        }
        private void UpdateNotificationForm_Shown(object sender, EventArgs e)
        {
            Deactivate += (s, a) => WindowFlasher.FlashWindowEx(Handle, WindowFlasher.FLASHW_TRAY);
            Activated += (s, a) => WindowFlasher.FlashWindowEx(Handle, WindowFlasher.FLASHW_STOP);
            SystemSounds.Asterisk.Play();
            updateButton.Focus();
        }
        private void updateButton_Click(object sender, EventArgs e)
        {
            updateButton.Enabled = false;

            if (!downloadUrl.Equals(string.Empty))
            {
                changeLogTextBox.Text = "";
                AppendChangeLog("Starting download... size " + FileSuffix(downloadSize));
                DownloadUpdate();
            }
        }
        private void cancelButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void UpdateNotificationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesDownloaded = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesDownloaded / totalBytes;

            downloadProgress++;

            if (downloadProgress % 15 == 0)
            {
                AppendChangeLog("Download " + Math.Truncate(percentage * 100) + "%");
            }
        }
        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            AppendChangeLog("Download completed... restarting");
            OpenUpdate();
        }
        private void DownloadUpdate()
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadProgressChanged += webClient_DownloadProgressChanged;
                    webClient.DownloadFileCompleted += webClient_DownloadFileCompleted;

                    webClient.DownloadFileAsync(new Uri(downloadUrl), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"overwatchtracker\overwatchtracker.exe"));
                }
                catch
                {
                    // ignored
                }
            }
        }
        public void OpenUpdate()
        {
            string argument = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\" & Start \"\" /D \"{3}\" \"{4}\" {5}";
            string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"overwatchtracker\overwatchtracker.exe");
            string currentPath = Application.ExecutablePath;

            ProcessStartInfo info = new ProcessStartInfo
            {
                Arguments = string.Format(argument, currentPath, tempPath, currentPath, Path.GetDirectoryName(currentPath), Path.GetFileName(currentPath), ""),
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(info);
            Environment.Exit(0);
        }
        private void AppendChangeLog(string message)
        {
            changeLogTextBox.AppendText(message + Environment.NewLine);
        }
        private static string FileSuffix(int fileSize)
        {
            if((double)fileSize / 1024 / 1024 > 0)
            {
                return Math.Round((double)fileSize / 1024 / 1024, 2) + " MB";
            }
            if((double)fileSize / 1024 > 0)
            {
                return Math.Round((double)fileSize / 1024, 2) + " KB";
            }
            return fileSize + " B";
        }
    }
    public class WindowFlasher
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
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public const int FLASHW_STOP = 0;
        public const int FLASHW_CAPTION = 1;
        public const int FLASHW_TRAY = 2;
        public const int FLASHW_ALL = 3;
        public const int FLASHW_TIMER = 4;
        public const int FLASHW_TIMERNOFG = 12;
        

        public static bool FlashWindowEx(IntPtr hWnd, int flags)
        {
            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = hWnd;
            fInfo.dwFlags = flags;
            fInfo.uCount = int.MaxValue;
            fInfo.dwTimeout = 0;

            return FlashWindowEx(ref fInfo);
        }
    }
}
