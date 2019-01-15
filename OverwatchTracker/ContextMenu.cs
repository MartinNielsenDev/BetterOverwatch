using System;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

namespace OverwatchTracker
{
    public class ContextMenu : Form
    {
        public MenuItem currentGame = new MenuItem("Last Game");
        public NotifyIcon trayIcon = new NotifyIcon();
        public System.Windows.Forms.ContextMenu trayMenu = new System.Windows.Forms.ContextMenu();
        private RegistryKey reg = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        public ContextMenu()
        {
            try
            {
                currentGame.MenuItems.Add("Time elapsed: --:--");
                currentGame.MenuItems.Add("Skill rating: ----");
                currentGame.MenuItems.Add("Map: ----");
                currentGame.MenuItems.Add("Teams rating: ---- | ----");
                currentGame.MenuItems.Add("Last Hero: ----");
                currentGame.MenuItems.Add("Final score: - | -");

                for(int i = 0; i < currentGame.MenuItems.Count; i++) { currentGame.MenuItems[i].Enabled = false; }
                MenuItem debugTools = new MenuItem("Debug tools");
                debugTools.MenuItems.Add(currentGame);
                
                trayMenu.MenuItems.Add("Overwatch Tracker v" + Vars.version);
                trayMenu.MenuItems.Add("Login", openMatchHistory);
                trayMenu.MenuItems.Add("-");
                trayMenu.MenuItems.Add("Upload screenshot of player list", toggleUpload);
                trayMenu.MenuItems.Add("Start when Windows starts", toggleWindows);
                trayMenu.MenuItems.Add("Play audio on success", toggleAudio);
                trayMenu.MenuItems.Add(debugTools);
                trayMenu.MenuItems.Add("-");
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
                        if (key != null)
                        {
                            key.SetValue("OverwatchTracker", "\"" + Application.ExecutablePath.ToString() + "\"");
                        }
                    }
                }
                if (Vars.settings.playAudioOnSuccess)
                {
                    trayMenu.MenuItems[5].Checked = true;
                }

                trayIcon.Text = "Waiting for Overwatch, idle...";
                trayIcon.Icon = Properties.Resources.Idle;
                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;
                trayIcon.DoubleClick += new EventHandler(openMatchHistory);
            }
            catch
            {
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        public void trayPopup(string title, string text, int timeout)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string, string, int>(trayPopup), new object[] { title, text, timeout });
                return;
            }
            Functions.DebugMessage(title);
            trayIcon.ShowBalloonTip(timeout, title, text, ToolTipIcon.None);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void toggleUpload(object sender, EventArgs e)
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
        private void toggleWindows(object sender, EventArgs e)
        {
            if (trayMenu.MenuItems[4].Checked)
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key != null)
                    {
                        key.DeleteValue("OverwatchTracker");
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
                        key.SetValue("OverwatchTracker", "\"" + Application.ExecutablePath.ToString() + "\"");
                    }
                }
                trayMenu.MenuItems[4].Checked = true;
            }
            Vars.settings.startWithWindows = trayMenu.MenuItems[4].Checked;
            Settings.Save();
        }
        private void toggleAudio(object sender, EventArgs e)
        {
            if (trayMenu.MenuItems[5].Checked)
            {
                trayMenu.MenuItems[5].Checked = false;
            }
            else
            {
                trayMenu.MenuItems[5].Checked = true;
            }
            Vars.settings.playAudioOnSuccess = trayMenu.MenuItems[5].Checked;
            Settings.Save();
        }
        private void openMatchHistory(object sender, EventArgs e)
        {
            if(Vars.publicId.Equals(String.Empty))
            {
                string token = Server.getToken(false);
                if (token.Contains("success"))
                {
                    Vars.publicId = token.Replace("success", "");
                    Functions.DebugMessage("Retrieved publicId: " + Vars.publicId);
                }
            }
            if (!Vars.publicId.Equals(String.Empty))
            {
                Process.Start(Vars.host + "/" + Vars.publicId + "?login=" + Vars.settings.privateToken);
            }
            else
            {
                Process.Start(Vars.host + "/new-account/?privateToken=" + Vars.settings.privateToken);
            }
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