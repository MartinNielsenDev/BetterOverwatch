using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace OverwatchTracker
{
    class Server
    {
        public static Stopwatch autoUpdaterTimer = new Stopwatch();
        public static void AutoUpdater()
        {
            if (autoUpdaterTimer.ElapsedMilliseconds >= 600000)
            {
                NewestVersionV2(firstRun: false);
                autoUpdaterTimer.Restart();
            }
        }
        public static void OpenUpdate()
        {
            string argument = "/C choice /C Y /N /D Y /T 4 & Del /F /Q \"{0}\" & choice /C Y /N /D Y /T 2 & Move /Y \"{1}\" \"{2}\" & Start \"\" /D \"{3}\" \"{4}\" {5}";
            string tempPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"overwatchtracker\overwatchtracker.exe");
            string currentPath = Application.ExecutablePath;

            ProcessStartInfo Info = new ProcessStartInfo();
            Info.Arguments = String.Format(argument, currentPath, tempPath, currentPath, Path.GetDirectoryName(currentPath), Path.GetFileName(currentPath), "");
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            Application.Exit();
        }
        public static bool DownloadUpdate(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.DownloadFile(url, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"overwatchtracker\overwatchtracker.exe"));
                    return true;
                }
                catch { }
            }
            return false;
        }
        public static void FetchBlizzardAppOffset()
        {
            try
            {
                string serverResponse;
                using (var client = new WebClient())
                {
                    serverResponse = client.DownloadString(Vars.host + "/api/version.xml");
                }
                XmlDocument xmlTest = new XmlDocument();
                xmlTest.LoadXml(serverResponse);
                XmlNodeList xmlNodeList = xmlTest.SelectNodes("update");

                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    Vars.blizzardAppOffset = Convert.ToInt32(xmlNode.SelectSingleNode("blizzardapp").InnerText, 16);
                    Console.WriteLine(Vars.blizzardAppOffset);
                }
            }catch { }
        }
        public static bool NewestVersionV2(bool firstRun)
        {
            try
            {
                string serverResponse;
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0");
                    serverResponse = client.DownloadString("https://api.github.com/repos/MartinNielsenDev/OverwatchTracker/releases/latest");
                }
                GitHub.Json json = JsonConvert.DeserializeObject<GitHub.Json>(serverResponse);
                if (json.tag_name.Length == 7 && json.assets.Count > 0)
                {
                    DateTime serverDate = DateTime.ParseExact(json.tag_name, "yy.MMdd", null);
                    DateTime thisDate = DateTime.ParseExact(Vars.version, "yy.MMdd", null);

                    if ((thisDate - serverDate).TotalSeconds < 0)
                    {
                        Functions.DebugMessage("New update required: v" + json.tag_name);
                        UpdateNotificationForm updateForm = new UpdateNotificationForm();
                        updateForm.label2.Text = "Current Version: " + Vars.version;
                        updateForm.label3.Text = "Newest Version: " + json.tag_name;
                        updateForm.textBox1.Text = json.body;
                        updateForm.urlToDownload = json.assets[0].browser_download_url;
                        Application.Run(updateForm);
                        return false;
                    }
                }
                else
                {
                    if (firstRun)
                    {
                        MessageBox.Show("Failed to fetch update information through https://api.github.com/repos/MartinNielsenDev/OverwatchTracker/releases/latest\r\n\r\nPlease manually update if there are any issues", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch
            {
                if (firstRun)
                {
                    Functions.DebugMessage("error while validating version");
                    MessageBox.Show("Error while validating version", "Overwatch Tracker v" + Vars.version, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return true;
        }
        public static void UploadGame(string gameData)
        {
            Functions.DebugMessage("Uploading GameData...");
            Program.uploaderThread = new Thread(() =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (GameUploader(gameData))
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
            });
            Program.uploaderThread.Start();
        }
        public static bool GameUploader(string gameData)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues(Vars.host + "/api/v2/upload-game/", new NameValueCollection()
                    {
                        { "gameData", gameData }
                    });
                    ServerOutput result = JsonConvert.DeserializeObject<ServerOutput>(Encoding.Default.GetString(response));

                    if (result.success)
                    {
                        Functions.DebugMessage("Successfully uploaded game");
                        return true;
                    }
                    else
                    {
                        Functions.DebugMessage("Failed to upload game, message: " + result.message);
                    }
                }
            }
            catch { }
            return false;
        }
        public static bool FetchTokens()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues(Vars.host + "/api/v2/fetch-token/", new NameValueCollection()
                    {
                        { "privateToken", Vars.settings.privateToken },
                        { "publicToken", Vars.gameData.battletag }
                    });
                    ServerOutput result = JsonConvert.DeserializeObject<ServerOutput>(Encoding.Default.GetString(response));
                    if (result.success)
                    {
                        Vars.settings.privateToken = result.privateToken;
                        Vars.settings.publicToken = result.publicToken;
                        Settings.Save();
                        return true;
                    }
                    else
                    {
                        Functions.DebugMessage("Failed to fetch tokens, message: " + result.message);
                        MessageBox.Show("Failed to fetch tokens, message: " + result.message, "Overwatch Tracker error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch { }
            return false;
        }
    }
    class ServerOutput
    {
        [JsonProperty("success")]
        public bool success { get; set; }
        [JsonProperty("message")]
        public string message { get; set; }
        [JsonProperty("privateToken")]
        public string privateToken { get; set; }
        [JsonProperty("publicToken")]
        public string publicToken { get; set; }
    }
    class GitHub
    {
        public class Json
        {
            [JsonProperty("tag_name")]
            public string tag_name { get; set; }
            [JsonProperty("body")]
            public string body { get; set; }
            [JsonProperty("assets")]
            public List<Assets> assets { get; set; }
        }
        public class Assets
        {
            [JsonProperty("browser_download_url")]
            public string browser_download_url { get; set; }
        }
    }
}