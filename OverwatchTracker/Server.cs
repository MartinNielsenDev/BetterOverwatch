using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    class Initalize
    {
        public Initalize(string version, string host, string gitHubHost)
        {
            this.Version = version;
            this.Host = host;
            this.GitHubHost = gitHubHost;
        }
        public string Version { get; } = "";
        public string Host { get; } = "";
        public string GitHubHost { get; } = "";
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
        public static bool FetchBlizzardAppOffset(string version)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues("http://api." + Vars.initalize.Host + "/fetch-offset/", new NameValueCollection()
                    {
                        { "version", version }
                    });
                    ServerOutput.OffsetOutput result = JsonConvert.DeserializeObject<ServerOutput.OffsetOutput>(Encoding.UTF8.GetString(response));

                    if (result.success)
                    {
                        Vars.blizzardAppOffset = Convert.ToInt32(result.offset, 16);
                        return true;
                    }
                }
            }
            catch { }
            return false;
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
                for (int i = 0; i < 10; i++)
                {
                    if (GameUploader(gameData)) break;
                    Thread.Sleep(1000);
                }
            }).Start();
        }
        private static bool GameUploader(string gameData)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues("http://api." + Vars.initalize.Host + "/upload-game/", new NameValueCollection()
                    {
                        { "gameData", gameData }
                    });
                    ServerOutput.TokensOutput result = JsonConvert.DeserializeObject<ServerOutput.TokensOutput>(Encoding.UTF8.GetString(response));

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
        public static void VerifyToken()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues("http://api." + Vars.initalize.Host + "/verify-account/", new NameValueCollection()
                    {
                        { "privateToken", Vars.settings.privateToken }
                    });
                    ServerOutput.TokensOutput result = JsonConvert.DeserializeObject<ServerOutput.TokensOutput>(Encoding.UTF8.GetString(response));

                    if (result.success)
                    {
                        Vars.settings.privateToken = result.privateToken;

                        if (result.publicToken.Length > 0)
                        {
                            Vars.settings.publicToken = result.publicToken;
                        }
                        if(!result.isLinked)
                        {
                            Program.authorizeForm = new AuthorizeForm();
                            Program.authorizeForm.isLinking = true;
                            Program.authorizeForm.textLabel.Text = "You can now link your Battle.net account to your Better Overwatch\r\n\r\nYou will then be able to login to your Better Overwatch from any computer";
                            Program.authorizeForm.Show();
                        }
                        Program.captureDesktop = true;
                    }
                    else
                    {
                        Program.authorizeForm = new AuthorizeForm();
                        Program.authorizeForm.Show();
                    }
                }
            }
            catch { }
        }
        public static async Task StartLocalAuthServer()
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://127.0.0.1:8005/");
                listener.Start();
                HttpListenerContext context = await listener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                Stream output = response.OutputStream;
                string privateToken = request.QueryString["privateToken"];
                string publicToken = request.QueryString["publicToken"];
                byte[] buffer = { };

                if (privateToken != null && publicToken != null)
                {
                    Vars.settings.privateToken = privateToken;
                    Vars.settings.publicToken = publicToken;

                    if (Program.authorizeForm != null)
                    {
                        Program.authorizeForm.Close();
                    }
                    Settings.Save();
                    Program.captureDesktop = true;
                    buffer = Encoding.UTF8.GetBytes($"<html><meta http-equiv=\"refresh\" content=\"0; url = http://betteroverwatch.com/user/{publicToken}?auth_success=1\" /></html>");
                    Program.trayMenu.trayMenu.MenuItems[1].Text = "Logout";
                }
                else
                {
                    buffer = Encoding.UTF8.GetBytes("<html>Failed to authorize, please try again</html>");

                    if (Program.authorizeForm != null)
                    {
                        Program.authorizeForm.Show();
                        Program.authorizeForm.authorizeButton.Enabled = true;
                    }
                }

                response.ContentLength64 = buffer.Length;
                output.Write(buffer, 0, buffer.Length);

                output.Close();
                listener.Stop();
            }
            catch { }
        }
    }
    class ServerOutput
    {
        public class TokensOutput
        {
            [JsonProperty("success")]
            public bool success { get; set; }
            [JsonProperty("message")]
            public string message { get; set; }
            [JsonProperty("privateToken")]
            public string privateToken { get; set; }
            [JsonProperty("publicToken")]
            public string publicToken { get; set; }
            [JsonProperty("isLinked")]
            public bool isLinked { get; set; }
        }
        public class OffsetOutput
        {
            [JsonProperty("success")]
            public bool success { get; set; }
            [JsonProperty("message")]
            public string message { get; set; }
            [JsonProperty("offset")]
            public string offset { get; set; }
        }
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