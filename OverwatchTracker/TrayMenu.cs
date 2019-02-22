using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    public class TrayMenu : Form
    {
        public MenuItem currentGame = new MenuItem("Last Game");
        public NotifyIcon trayIcon = new NotifyIcon();
        public ContextMenu trayMenu = new ContextMenu();
        private readonly RegistryKey registry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        public TrayMenu()
        {
            try
            {
                currentGame.MenuItems.Add("Time elapsed: --:--");
                currentGame.MenuItems.Add("Skill rating: ----");
                currentGame.MenuItems.Add("Map: ----");
                currentGame.MenuItems.Add("Teams rating: ---- | ----");
                currentGame.MenuItems.Add("Last Hero: ----");
                currentGame.MenuItems.Add("Final score: - | -");
                for (int i = 0; i < currentGame.MenuItems.Count; i++)
                {
                    currentGame.MenuItems[i].Enabled = false;
                }
                MenuItem debugTools = new MenuItem("Tools");
                debugTools.MenuItems.Add("Open logs", OpenLogs);
                debugTools.MenuItems.Add("Export Last Game", FetchJSON);
                debugTools.MenuItems.Add(currentGame);

                trayMenu.MenuItems.Add("Better Overwatch v" + Vars.initalize.Version);
                trayMenu.MenuItems.Add("Login", Login);
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
                        if (key != null)
                        {
                            key.SetValue("BetterOverwatch", "\"" + Application.ExecutablePath.ToString() + "\"");
                        }
                    }
                }
                ChangeTray("Waiting for Overwatch, idle...", Properties.Resources.Idle);
                trayIcon.ContextMenu = trayMenu;
                trayIcon.Visible = true;
                trayIcon.DoubleClick += new EventHandler(OpenMatchHistory);
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
                this.Invoke(new Action<string, int>(TrayPopup), new object[] { text, timeout });
                return;
            }
            trayIcon.ShowBalloonTip(timeout, "Better Overwatch", text, ToolTipIcon.None);
        }
        private void FetchJSON(object sender, EventArgs e)
        {
            if (Vars.lastGameJSON.Length > 0)
            {
                string path = Path.Combine(Path.GetTempPath(), "lastgame.json");
                File.WriteAllText(path, Vars.lastGameJSON);
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
                        key.SetValue("BetterOverwatch", "\"" + Application.ExecutablePath.ToString() + "\"");
                    }
                }
                trayMenu.MenuItems[4].Checked = true;
            }
            Vars.settings.startWithWindows = trayMenu.MenuItems[4].Checked;
            Settings.Save();
        }
        private void OpenMatchHistory(object sender, EventArgs e)
        {
            if (!Vars.settings.publicToken.Equals(String.Empty))
            {
                Process.Start("http://" + Vars.initalize.Host + "/" + Vars.settings.publicToken);
            }
        }
        private async void Login(object sender, EventArgs e)
        {
            Process.Start("https://eu.battle.net/oauth/authorize?response_type=code&client_id=20d78829a4e641e694d8ec7f1198dc8b&redirect_uri=http://betteroverwatch.com/api/authorize/");
            await Server.StartLocalAuthServer();
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
    class CleanedGame
    {
#pragma warning disable 0169
        [JsonProperty("mapInfo")]
        private readonly Game.MapInfo mapInfo;
        [JsonProperty("startRating")]
        private readonly string startRating;
        [JsonProperty("endRating")]
        private readonly string endRating;
        [JsonProperty("team1Rating")]
        private readonly string team1Rating;
        [JsonProperty("team2Rating")]
        private readonly string team2Rating;
        [JsonProperty("team1Score")]
        private readonly string team1Score;
        [JsonProperty("team2Score")]
        private readonly string team2Score;
        [JsonProperty("duration")]
        private readonly string duration;
        [JsonProperty("battleTag")]
        private readonly string battleTag;
        [JsonProperty("heroes")]
        private readonly List<Game.HeroPlayed> heroes;
        [JsonProperty("players")]
        private readonly List<Game.Player> players;
        [JsonProperty("statsRecorded")]
        private readonly List<Game.Stats> statsRecorded;
    }
}