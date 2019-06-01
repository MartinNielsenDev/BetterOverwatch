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
        public ContextMenu trayMenu = new ContextMenu();

        public TrayMenu()
        {
            try
            {
                MenuItem debugTools = new MenuItem("Tools");
                //debugTools.MenuItems.Add("Start test", StartTest);
                debugTools.MenuItems.Add("Open logs", OpenLogs);
                debugTools.MenuItems.Add("Export Last Game", FetchJson);

                trayMenu.MenuItems.Add("Better Overwatch v" + Vars.initalize.Version);
                trayMenu.MenuItems.Add("Logout", LoginLogout);
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add("Upload screenshot of player list", ToggleUpload);
                trayMenu.MenuItems.Add("Start with Windows", ToggleWindows);
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add(debugTools);
                trayMenu.MenuItems.Add("Exit", OnExit);
                trayMenu.MenuItems[0].Enabled = false;

                if (Vars.settings.uploadScreenshot)
                {
                    trayMenu.MenuItems[3].Checked = true;
                }
                if (Vars.settings.startWithWindows)
                {
                    trayMenu.MenuItems[4].Checked = true;
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        key?.SetValue("BetterOverwatch", "\"" + Application.ExecutablePath + "\"");
                    }
                }
                ChangeTray("Waiting for Overwatch, idle...", Resources.Icon);
                trayIcon.ContextMenu = trayMenu;
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
            if (Vars.lastGameJSON.Length > 0)
            {
                string path = Path.Combine(Path.GetTempPath(), "last-game.json");
                File.WriteAllText(path, Vars.lastGameJSON);
                Process.Start("notepad.exe", path);
                TrayPopup("Last game successfully fetched", 3000);
            }
            else
            {
                TrayPopup("No game was found", 3000);
            }
        }
        private void StartTest(object sender, EventArgs e)
        {
            Vars.gameData = new Game.GameData(Vars.gameData.currentRating);
            Vars.getInfoTimeout.Restart();
            Vars.gameData.state = State.Ingame;
            Vars.gameData.startRating = Vars.gameData.currentRating;
            Console.WriteLine("Started");
        }
        private void OpenLogs(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(Vars.configPath, "debug.log")))
            {
                Process.Start(Path.Combine(Vars.configPath, "debug.log"));
            }
        }
        private void ToggleUpload(object sender, EventArgs e)
        {
            if (trayMenu.MenuItems[3].Checked)
            {
                trayMenu.MenuItems[3].Checked = false;
            }
            else
            {
                trayMenu.MenuItems[3].Checked = true;
            }
            Vars.settings.uploadScreenshot = trayMenu.MenuItems[3].Checked;
            Settings.Save();
        }
        private void ToggleWindows(object sender, EventArgs e)
        {
            if (trayMenu.MenuItems[4].Checked)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("BetterOverwatch");
                    }
                }
                trayMenu.MenuItems[4].Checked = false;
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
                trayMenu.MenuItems[4].Checked = true;
            }
            Vars.settings.startWithWindows = trayMenu.MenuItems[4].Checked;
            Settings.Save();
        }
        private void OpenMatchHistory(object sender, EventArgs e)
        {
            if (!Vars.settings.publicToken.Equals(string.Empty))
            {
                Process.Start($"http://{Vars.initalize.Host}/user/{Vars.settings.publicToken}");
            }
        }
        private async void LoginLogout(object sender, EventArgs e)
        {
            if (trayMenu.MenuItems[1].Text.Equals("Login"))
            {
                Process.Start("https://eu.battle.net/oauth/authorize?response_type=code&client_id=20d78829a4e641e694d8ec7f1198dc8b&redirect_uri=http://betteroverwatch.com/api/authorize/");
                await Server.StartLocalAuthServer();
            }
            else
            {
                trayMenu.MenuItems[1].Text = "Login";
                Process.Start("http://betteroverwatch.com/logout.php");
                File.Delete(Path.Combine(Vars.configPath, "settings.json"));
                Vars.settings = new Settings();
                Program.captureDesktop = false;
                Program.authorizeForm = new AuthorizeForm
                {
                    textLabel =
                    {
                        Text = "Logout successful and settings cleared\r\n\r\nYou must authorize to continue using Better Overwatch"
                    }
                };
                Program.authorizeForm.Show();
            }
        }
        public void ChangeTray(string text, Icon icon)
        {
            Console.WriteLine(text);
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