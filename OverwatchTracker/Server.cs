using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    class Initalize
    {
        private string version = "";
        private string host = "";
        private string gitHubHost = "";

        public Initalize(string version, string host, string gitHubHost)
        {
            this.version = version;
            this.host = host;
            this.gitHubHost = gitHubHost;
        }
        public string Version
        {
            get { return version; }
        }
        public string Host
        {
            get { return host; }
        }
        public string GitHubHost
        {
            get { return gitHubHost; }
        }
    }
    class Server
    {
        public static Stopwatch autoUpdaterTimer = new Stopwatch();
        public static void AutoUpdater()
        {
            if (autoUpdaterTimer.ElapsedMilliseconds / 1000 >= 600)
            {
                CheckNewestVersion();
                autoUpdaterTimer.Restart();
            }
        }
        public static void FetchBlizzardAppOffset()
        {
            try
            {
                string serverResponse;
                using (var client = new WebClient())
                {
                    serverResponse = client.DownloadString("http://api." + Vars.initalize.Host + "/version.xml");
                }
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(serverResponse);
                XmlNodeList xmlNodeList = xmlDocument.SelectNodes("update");

                foreach (XmlNode xmlNode in xmlNodeList)
                {
                    Vars.blizzardAppOffset = Convert.ToInt32(xmlNode.SelectSingleNode("blizzardapp").InnerText, 16);
                }
            }
            catch { }
        }
        public static bool CheckNewestVersion()
        {
            try
            {
                string serverResponse;
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0");
                    serverResponse = client.DownloadString(Vars.initalize.GitHubHost);
                }
                GitHub.Json json = JsonConvert.DeserializeObject<GitHub.Json>(serverResponse);

                if (json.tag_name.Split('.').Length == 3 && json.assets.Count > 0)
                {
                    Versioning thisVersion = new Versioning(Vars.initalize.Version);
                    Versioning serverVersion = new Versioning(json.tag_name);

                    if (serverVersion.IsNewerThan(thisVersion))
                    {
                        Functions.DebugMessage("New update required: v" + json.tag_name);
                        UpdateNotificationForm updateForm = new UpdateNotificationForm();
                        updateForm.installedVersionLabel.Text += Vars.initalize.Version;
                        updateForm.updateVersionLabel.Text += json.tag_name;
                        updateForm.titleSubLabel.Text += json.tag_name;
                        updateForm.changeLogTextBox.Text = json.body;
                        updateForm.downloadUrl = json.assets[0].browser_download_url;
                        updateForm.downloadSize = json.assets[0].size;
                        Application.Run(updateForm);
                        return false;
                    }
                }
            }
            catch { }
            return true;
        }
        public static void UploadGame(string gameData)
        {
            Functions.DebugMessage("Uploading GameData...");
            new Thread(() =>
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (GameUploader(gameData))
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }
            }).Start();
        }
        public static bool GameUploader(string gameData)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues("http://api." + Vars.initalize.Host + "/v2/upload-game/", new NameValueCollection()
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
                    byte[] response = client.UploadValues("http://api." + Vars.initalize.Host + "/v2/fetch-token/", new NameValueCollection()
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
                        MessageBox.Show("Failed to fetch tokens, message: " + result.message, "Better Overwatch error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            [JsonProperty("size")]
            public int size { get; set; }
        }
    }
    class Versioning
    {
        public int major = 0;
        public int minor = 0;
        public int patch = 0;

        public Versioning(string rawVersion)
        {
            string[] versions = rawVersion.Split('.');

            if (versions.Length == 3)
            {
                int.TryParse(versions[0], out this.major);
                int.TryParse(versions[1], out this.minor);
                int.TryParse(versions[2], out this.patch);
            }
        }
        public bool IsNewerThan(Versioning version)
        {
            if (this.major > version.major)
            {
                return true;
            }
            else if (this.major == version.major)
            {
                if (this.minor > version.minor)
                {
                    return true;
                }
                else if (this.minor == version.minor && this.patch > version.patch)
                {
                    return true;
                }
            }

            return false;
        }
    }
}