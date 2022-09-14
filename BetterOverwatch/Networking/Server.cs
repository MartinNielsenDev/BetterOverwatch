using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BetterOverwatch.DataObjects;
using BetterOverwatch.Forms;
using BetterOverwatch.Properties;
using ConvNeuralNetwork;
using Newtonsoft.Json;

namespace BetterOverwatch.Networking
{
    class Server
    {
        internal static Stopwatch autoUpdaterTimer = new Stopwatch();
        internal static void AutoUpdater()
        {
            if (autoUpdaterTimer.ElapsedMilliseconds / 1000 >= 600)
            {
                CheckNewestVersion();
                autoUpdaterTimer.Restart();
            }
        }
        internal static bool FetchNetworks()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues($"https://api.{AppData.initalize.Host}/network/", new NameValueCollection
                    {
                        { "version", AppData.settings.networkVersion.ToString() }
                    });
                    ServerOutput.NetworksOutput result = JsonConvert.DeserializeObject<ServerOutput.NetworksOutput>(Encoding.UTF8.GetString(response));

                    if (result.success)
                    {
                        using (var md5 = MD5.Create())
                        {
                            for (int i = 0; i < result.networks.Length; i++)
                            {
                                if (result.networks[i] != "false")
                                {
                                    File.WriteAllText(Path.Combine(AppData.configPath, "_data", $"network{i}"), result.networks[i]);
                                }
                            }
                            AppData.settings.networkVersion = result.version;
                        }
                    }
                }
            }
            catch { }
            try
            {
                AppData.networks = new ConvNet[]
                {
                    new ConvNet(Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(Path.Combine(AppData.configPath, "_data", "network0"))))),
                    new ConvNet(Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(Path.Combine(AppData.configPath, "_data", "network1"))))),
                    new ConvNet(Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(Path.Combine(AppData.configPath, "_data", "network2"))))),
                    new ConvNet(Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(Path.Combine(AppData.configPath, "_data", "network3"))))),
                    new ConvNet(Encoding.UTF8.GetString(Convert.FromBase64String(File.ReadAllText(Path.Combine(AppData.configPath, "_data", "network4")))))
                };
                Settings.Save();
                return true;
            }
            catch (Exception e)
            {
                Functions.DebugMessage($"Could not start BetterOverwatch: {e.Message}");
                return false;
            }
        }
        internal static void VerifyToken()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues($"https://api.{AppData.initalize.Host}/verify-account/", new NameValueCollection
                    {
                        { "privateToken", AppData.settings.privateToken }
                    });
                    ServerOutput.TokensOutput result = JsonConvert.DeserializeObject<ServerOutput.TokensOutput>(Encoding.UTF8.GetString(response));

                    if (result.success)
                    {
                        AppData.settings.privateToken = result.privateToken;

                        if (result.publicToken.Length > 0)
                        {
                            AppData.settings.publicToken = result.publicToken;
                        }
                        ScreenCaptureHandler.captureScreen = true;
                        ScreenCaptureHandler.trayMenu.contextMenu.MenuItems[1].Text = "Logout";
                    }
                    else
                    {
                        Program.autenticationForm = new AuthenticationForm();
                        Program.autenticationForm.Show();
                    }
                }
            }
            catch
            {
                Program.autenticationForm = new AuthenticationForm();
                Program.autenticationForm.Show();
            }
        }
        internal static bool FetchBlizzardAppOffset(string version)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] response = client.UploadValues($"https://api.{AppData.initalize.Host}/fetch-offset/", new NameValueCollection
                    {
                        { "version", version }
                    });
                    ServerOutput.OffsetOutput result = JsonConvert.DeserializeObject<ServerOutput.OffsetOutput>(Encoding.UTF8.GetString(response));

                    if (result.success)
                    {
                        AppData.blizzardAppOffset = Convert.ToInt32(result.offset, 16);
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }
        internal static bool CheckNewestVersion()
        {
            try
            {
                string serverResponse;
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Mozilla/5.0");
                    serverResponse = client.DownloadString(AppData.initalize.GitHubHost);
                }
                GitHub.Json json = JsonConvert.DeserializeObject<GitHub.Json>(serverResponse);

                if (json.tag_name.Split('.').Length == 3 && json.assets.Count > 0)
                {
                    SemanticVersion thisVersion = new SemanticVersion(AppData.initalize.Version);
                    SemanticVersion serverVersion = new SemanticVersion(json.tag_name);

                    if (serverVersion.IsNewerThan(thisVersion))
                    {
                        Functions.DebugMessage("New update required: v" + json.tag_name);
                        UpdateNotificationForm updateForm = new UpdateNotificationForm();
                        updateForm.installedVersionLabel.Text += AppData.initalize.Version;
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
        internal static void CalculateStats()
        {
            if (AppData.gameData.team1Score != 0 && AppData.gameData.team2Score != 0)
            {
                if (AppData.gameData.team1Score > AppData.gameData.team2Score) // win
                {
                    AppData.win++;
                }
                else if (AppData.gameData.team1Score < AppData.gameData.team2Score) // loss
                {
                    AppData.loss++;
                }
                else
                {
                    AppData.draw++;
                }
            }
            else if (AppData.gameData.endRating - AppData.gameData.startRating > 0) // win
            {
                AppData.win++;
            }
            else if (AppData.gameData.endRating - AppData.gameData.startRating < 0) // loss
            {
                AppData.loss++;
            }
            else if (AppData.gameData.endRating - AppData.gameData.startRating == 0 && AppData.gameData.startRating + AppData.gameData.endRating != 0) // draw
            {
                AppData.draw++;
            }
        }
        internal static void CheckGameUpload()
        {
            string game = AppData.gameData.ToString();
            AppData.lastGameJSON = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<CompetitiveGame>(game), Formatting.Indented);
            if (!GameMethods.IsValidGame()) return;
            CalculateStats();

            if (AppData.settings.outputStatsToTextFile)
            {
                try
                {
                    string outputStats = ScreenCaptureHandler.trayMenu.winratesForm.textBox1.Text
                        .Replace("{win}", AppData.win.ToString())
                        .Replace("{loss}", AppData.loss.ToString())
                        .Replace("{draw}", AppData.draw.ToString())
                        .Replace("{wr}", Math.Round((double)AppData.win / (double)(AppData.win + AppData.loss + AppData.draw) * 100, 2).ToString());
                    File.WriteAllText("stats.txt", outputStats);
                }
                catch { }
            }
            UploadGame(game);
        }
        internal static void UploadGame(string gameData)
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
                            byte[] response = client.UploadValues($"https://api.{AppData.initalize.Host}/game/upload/", new NameValueCollection {
                                { "gameData", gameData }
                            });
                            ServerOutput.TokensOutput result = JsonConvert.DeserializeObject<ServerOutput.TokensOutput>(Encoding.UTF8.GetString(response));

                            if (result.success)
                            {
                                ScreenCaptureHandler.trayMenu.ChangeTray("Previous game successfully uploaded", Resources.Icon_Active);
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
        internal static async Task StartLocalAuthServer()
        {
            // this local server is for capturing the privateToken and publicToken from the server through the user's browser
            try
            {
                HttpListener httpListener = new HttpListener();
                httpListener.Prefixes.Add("http://127.0.0.1:8005/");
                httpListener.Start();
                HttpListenerContext context = await httpListener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                Stream output = response.OutputStream;
                string privateToken = request.QueryString["privateToken"];
                string publicToken = request.QueryString["publicToken"];
                byte[] buffer = { };

                if (privateToken != null && publicToken != null)
                {
                    AppData.settings.privateToken = privateToken;
                    AppData.settings.publicToken = publicToken;

                    if (Program.autenticationForm != null) Program.autenticationForm.Close();
                    Settings.Save();
                    ScreenCaptureHandler.captureScreen = true;
                    buffer = Encoding.UTF8.GetBytes($"<html><meta http-equiv=\"refresh\" content=\"0; url = http://betteroverwatch.com/user/{publicToken}?auth_success=1\" /></html>");
                    ScreenCaptureHandler.trayMenu.contextMenu.MenuItems[1].Text = "Logout";
                    Functions.DebugMessage($"Successfully authenticated user '{publicToken}'");
                    Program.autenticationForm.Hide();
                }
                else
                {
                    buffer = Encoding.UTF8.GetBytes("<html>Failed to authenticate, please try again</html>");
                    Functions.DebugMessage("Failed to authenticate");

                    if (Program.autenticationForm != null)
                    {
                        Program.autenticationForm.Show();
                    }
                }
                response.ContentLength64 = buffer.Length;
                output.Write(buffer, 0, buffer.Length);
                Thread.Sleep(1500);
                output.Close();
                httpListener.Stop();
            }
            catch { }
        }
    }
}