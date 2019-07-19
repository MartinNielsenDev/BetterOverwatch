using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BetterOverwatch.Forms;
using BetterOverwatch.Networking;
using BetterOverwatch.Properties;
using Microsoft.Win32;

namespace BetterOverwatch
{
    public class TrayMenu : Form
    {
        public NotifyIcon trayIcon = new NotifyIcon();
        public ContextMenu contextMenu = new ContextMenu();

        public TrayMenu()
        {
            try
            {
                MenuItem debugTools = new MenuItem("Tools");
                debugTools.MenuItems.Add("Open logs", OpenLogs);
                debugTools.MenuItems.Add("Export Last Game", FetchJson);

                contextMenu.MenuItems.Add("Better Overwatch v" + AppData.initalize.Version);
                contextMenu.MenuItems.Add("Login", LoginLogout);
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add("Upload screenshot of player list", ToggleUpload);
                contextMenu.MenuItems.Add("Start with Windows", ToggleWindows);
                contextMenu.MenuItems.Add("-");
                contextMenu.MenuItems.Add(debugTools);
                contextMenu.MenuItems.Add("Exit", OnExit);
                contextMenu.MenuItems[0].Enabled = false;

                if (AppData.settings.uploadScreenshot)
                {
                    contextMenu.MenuItems[3].Checked = true;
                }
                if (AppData.settings.startWithWindows)
                {
                    contextMenu.MenuItems[4].Checked = true;
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        key?.SetValue("BetterOverwatch", "\"" + Application.ExecutablePath + "\"");
                    }
                }
                ChangeTray("Waiting for Overwatch, idle...", Resources.Icon);
                trayIcon.ContextMenu = contextMenu;
                trayIcon.Visible = true;
                trayIcon.DoubleClick += OpenMatchHistory;
            }
            catch { }
        }
        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }
        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void TrayPopup(string text, int timeout)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, int>(TrayPopup), text, timeout);
                return;
            }
            trayIcon.ShowBalloonTip(timeout, "Better Overwatch", text, ToolTipIcon.None);
        }
        private void FetchJson(object sender, EventArgs e)
        {
            if (AppData.lastGameJSON.Length > 0)
            {
                string path = Path.Combine(Path.GetTempPath(), "last-game.json");
                File.WriteAllText(path, AppData.lastGameJSON);
                Process.Start("notepad.exe", path);
                TrayPopup("Last game successfully fetched", 3000);
            }
            else
            {
                TrayPopup("No game was found", 3000);
            }
        }
        private void OpenLogs(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(AppData.configPath, "debug.log")))
            {
                Process.Start(Path.Combine(AppData.configPath, "debug.log"));
            }
        }
        private void ToggleUpload(object sender, EventArgs e)
        {
            if (contextMenu.MenuItems[3].Checked)
            {
                contextMenu.MenuItems[3].Checked = false;
            }
            else
            {
                contextMenu.MenuItems[3].Checked = true;
            }
            AppData.settings.uploadScreenshot = contextMenu.MenuItems[3].Checked;
            Settings.Save();
        }
        private void ToggleWindows(object sender, EventArgs e)
        {
            if (contextMenu.MenuItems[4].Checked)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("BetterOverwatch");
                    }
                }
                contextMenu.MenuItems[4].Checked = false;
            }
            else
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.SetValue("BetterOverwatch", "\"" + Application.ExecutablePath + "\"");
                    }
                }
                contextMenu.MenuItems[4].Checked = true;
            }
            AppData.settings.startWithWindows = contextMenu.MenuItems[4].Checked;
            Settings.Save();
        }
        private void OpenMatchHistory(object sender, EventArgs e)
        {
            if (!AppData.settings.publicToken.Equals(string.Empty))
            {
                Process.Start($"http://{AppData.initalize.Host}/user/{AppData.settings.publicToken}");
            }
        }
        private async void LoginLogout(object sender, EventArgs e)
        {
            if (contextMenu.MenuItems[1].Text.Equals("Login"))
            {
                Process.Start("https://eu.battle.net/oauth/authorize?response_type=code&client_id=20d78829a4e641e694d8ec7f1198dc8b&redirect_uri=http://betteroverwatch.com/api/authorize/");
                await Server.StartLocalAuthServer();
            }
            else
            {
                contextMenu.MenuItems[1].Text = "Login";
                Process.Start("http://betteroverwatch.com/logout.php");
                File.Delete(Path.Combine(AppData.configPath, "settings.json"));
                AppData.settings = new Settings();
                ScreenCaptureHandler.captureScreen = false;
                Program.autenticationForm = new AuthenticationForm
                {
                    textLabel =
                    {
                        Text = "Logout successful and settings cleared\r\n\r\nYou must authorize to continue using Better Overwatch"
                    }
                };
                Program.autenticationForm.Show();
            }
        }
        public void ChangeTray(string text, Icon icon)
        {
            TrayPopup(text, 5000);
            trayIcon.Text = text;
            trayIcon.Icon = icon;
        }
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }
    }
}