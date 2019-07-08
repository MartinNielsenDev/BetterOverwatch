using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BetterOverwatch.DataObjects;
using BetterOverwatch.Forms;
using Newtonsoft.Json;

namespace BetterOverwatch.Networking
{
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
        public static void VerifyToken()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues($"http://api.{Vars.initalize.Host}/verify-account/", new NameValueCollection
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
                        if (!result.isLinked)
                        {
                            Program.authorizeForm = new AuthorizeForm
                            {
                                isLinking = true
                            };
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
            catch {
                Program.authorizeForm = new AuthorizeForm();
                Program.authorizeForm.Show();
            }
        }
        public static bool FetchBlizzardAppOffset(string version)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues($"http://api.{Vars.initalize.Host}/fetch-offset/", new NameValueCollection
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
                    SemanticVersion thisVersion = new SemanticVersion(Vars.initalize.Version);
                    SemanticVersion serverVersion = new SemanticVersion(json.tag_name);

                    if (serverVersion.IsNewerThan(thisVersion))
                    {
                        Functions.DebugMessage("New update required: v" + json.tag_name);
                        UpdateNotificationForm updateForm = new UpdateNotificationForm();
                        updateForm.installedVersionLabel.Text += Vars.initalize.Version;
                        updateForm.updateVersionLabel.Text += json.tag_name;
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
        public static void CheckGameUpload()
        {
            string game = Vars.gameData.ToString();
            Vars.lastGameJSON = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<CompetitiveGame>(game), Formatting.Indented);
            if (!GameMethods.IsValidGame()) return;
            UploadGame(game);
        }
        public static void UploadGame(string gameData)
        {
            Functions.DebugMessage("Uploading GameData...");
            new Thread(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            byte[] response = client.UploadValues($"http://api.{Vars.initalize.Host}/game/upload/", new NameValueCollection {
                                { "gameData", gameData }
                            });
                            ServerOutput.TokensOutput result = JsonConvert.DeserializeObject<ServerOutput.TokensOutput>(Encoding.UTF8.GetString(response));

                            if (result.success)
                            {
                                Functions.DebugMessage("Successfully uploaded game");
                                break;
                            }
                            Functions.DebugMessage("Failed to upload game, message: " + result.message);
                        }
                    }
                    catch { }
                    Thread.Sleep(1000);
                }
            }).Start();
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
                Thread.Sleep(1500);
                output.Close();
                listener.Stop();
            }
            catch { }
        }
    }
}