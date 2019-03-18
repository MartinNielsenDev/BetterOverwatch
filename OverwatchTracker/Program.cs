using System;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Security.Principal;
using System.Net;
using DesktopDuplication;

namespace BetterOverwatch
{
    class Program
    {
        public static bool captureDesktop = false;
        public static TrayMenu trayMenu;
        public static AuthorizeForm authorizeForm;
        public static AdminPromptForm adminPromptForm;
        private static DesktopDuplicator desktopDuplicator;
        private static Mutex mutex = new Mutex(true, "74bf6260-c133-4d69-ad9c-efc607887c97");
        [STAThread]
        static void Main()
        {
            Vars.initalize = new Initalize(
                version: "1.2.3",
                host: "betteroverwatch.com",
                gitHubHost: "https://api.github.com/repos/MartinNielsenDev/OverwatchTracker/releases/latest"
                );
            Functions.DebugMessage("Starting Better Overwatch version " + Vars.initalize.Version);

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Better Overwatch is already running\r\n\r\nYou must close other instances of Better Overwatch if you want to open this one", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((s, assembly) =>
            {
                if (assembly.Name.Contains("Newtonsoft.Json,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.Newtonsoft.Json.dll");
                }
                if (assembly.Name.Contains("NeuralNetwork,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.NeuralNetwork.dll");
                }
                if (assembly.Name.Contains("AForge.Imaging,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.AForge.Imaging.dll");
                }
                if (assembly.Name.Contains("SharpDX.Direct3D11,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.SharpDX.Direct3D11.dll");
                }
                if (assembly.Name.Contains("SharpDX.DXGI,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.SharpDX.DXGI.dll");
                }
                if (assembly.Name.Contains("SharpDX,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.SharpDX.dll");
                }
                if (assembly.Name.Contains("System,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.System.dll");
                }
                if (assembly.Name.Contains("System.Drawing,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.System.Drawing.dll");
                }
                if (assembly.Name.Contains("System.Windows.Forms,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.System.Windows.Forms.dll");
                }
                if (assembly.Name.Contains("System.Xml,"))
                {
                    return LoadAssembly("BetterOverwatch.Resources.System.Xml.dll");
                }
                return null;
            });
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    Vars.isAdmin = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                }
                if (!Server.CheckNewestVersion()) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Directory.CreateDirectory(Vars.configPath);
                Vars.settings = new Settings();
                Settings.Load();
                Vars.gameData = new Game();
                trayMenu = new TrayMenu();
                Server.autoUpdaterTimer.Start();
                Functions.DebugMessage("> success - Better Overwatch started");
            }
            catch (Exception e)
            {
                MessageBox.Show("Startup error: " + e.ToString() + "\r\n\r\nReport this on the discord server");
                Environment.Exit(0);
                return;
            }
            if (!Vars.isAdmin)
            {
                adminPromptForm = new AdminPromptForm();
                adminPromptForm.Show();
            }
            else
            {
                Server.VerifyToken();
            }
            new Thread(CaptureDesktop) { IsBackground = true }.Start();
            Application.Run(trayMenu);
        }
        public static void CaptureDesktop()
        {
            Vars.statsTimer.Restart();
            try
            {
                desktopDuplicator = new DesktopDuplicator(0);
                Vars.frameTimer.Start();
            }
            catch (Exception e)
            {
                Functions.DebugMessage("Could not initialize desktopDuplication API - shutting down:" + e.ToString());
                Environment.Exit(0);
            }
            while (true)
            {
                if (captureDesktop)
                {
                    if (!Functions.ActiveWindowTitle().Equals("Overwatch"))
                    {
                        if (!Vars.overwatchRunning)
                        {
                            if (Functions.IsProcessOpen("Overwatch"))
                            {
                                trayMenu.ChangeTray("Visit play menu to update your skill rating", Properties.Resources.IconVisitMenu);
                                Vars.overwatchRunning = true;
                            }
                            else
                            {
                                Server.AutoUpdater();
                            }
                        }
                        else
                        {
                            if (!Functions.IsProcessOpen("Overwatch"))
                            {
                                trayMenu.ChangeTray("Waiting for Overwatch, idle...", Properties.Resources.Idle);
                                Vars.overwatchRunning = false;
                                if (Vars.gameData.state == State.Recording || 
                                    Vars.gameData.state == State.Finished || 
                                    Vars.gameData.state == State.WaitForUpload
                                    )
                                {
                                    Protocols.CheckGameUpload();
                                }
                            }
                        }
                        Thread.Sleep(500);
                    }
                    else
                    {
                        Thread.Sleep(50);
                        trayMenu.currentGame.MenuItems[0].Text = "Game elapsed: " + Functions.SecondsToMinutes((int)Math.Floor(Convert.ToDouble(Vars.gameTimer.ElapsedMilliseconds / 1000)));

                        if (Vars.frameTimer.ElapsedMilliseconds >= Vars.loopDelay)
                        {
                            DesktopFrame frame = null;

                            try
                            {
                                frame = desktopDuplicator.GetLatestFrame();
                            }
                            catch
                            {
                                desktopDuplicator.Reinitialize();
                                continue;
                            }
                            if (frame != null)
                            {
                                try
                                {
                                    if (Vars.gameData.state != State.Ingame)
                                    {
                                        string quickPlayText = Functions.BitmapToText(frame.DesktopImage, 476, 644, 80, 40, contrastFirst: false, radius: 140, network: Network.Maps, invertColors: true);

                                        if (Functions.CompareStrings(quickPlayText, "PLHY") >= 70)
                                        {
                                            Thread.Sleep(500);
                                            Protocols.CheckPlayMenu(frame.DesktopImage);
                                        }
                                    }

                                    if (Vars.gameData.state == State.Idle || Vars.gameData.state == State.Finished || Vars.gameData.state == State.WaitForUpload)
                                    {
                                        Protocols.CheckCompetitiveGameEntered(frame.DesktopImage);
                                    }

                                    if (Vars.gameData.state == State.Ingame)
                                    {
                                        Protocols.CheckMap(frame.DesktopImage);
                                        Protocols.CheckTeamsSkillRating(frame.DesktopImage);

                                        if (!Vars.gameData.team1Rating.Equals(String.Empty) &&
                                            !Vars.gameData.team2Rating.Equals(String.Empty) &&
                                            !Vars.gameData.mapInfo.Equals(String.Empty) ||
                                            !Vars.gameData.mapInfo.Equals(String.Empty) && Vars.getInfoTimeout.ElapsedMilliseconds >= 4000)
                                        {
                                            if (Vars.gameData.playerListImage == null)
                                            {
                                                Thread.Sleep(Vars.getInfoTimeout.ElapsedMilliseconds >= 4000 ? 0 : 2000);
                                                try
                                                {
                                                    frame = desktopDuplicator.GetLatestFrame();
                                                    Vars.gameData.playerListImage = new Bitmap(Functions.CaptureRegion(frame.DesktopImage, 0, 110, 1920, 700));
                                                    //Vars.gameData.debugImage = new Bitmap(frame.DesktopImage);
                                                    Protocols.CheckPlayerNamesAndRank(frame.DesktopImage);
                                                }
                                                catch { }
                                            }
                                            Vars.loopDelay = 500;
                                            Vars.gameData.state = State.Recording;
                                            trayMenu.ChangeTray("Recording... visit the main menu after the game", Properties.Resources.IconRecord);
                                            Vars.statsTimer.Restart();
                                            Vars.getInfoTimeout.Stop();
                                        }
                                        else if (Vars.getInfoTimeout.ElapsedMilliseconds >= 4500)
                                        {
                                            Vars.roundTimer.Stop();
                                            Vars.gameTimer.Stop();
                                            Vars.getInfoTimeout.Stop();
                                            Vars.gameData.state = State.Idle;
                                            Functions.DebugMessage("Failed to find game");
                                        }
                                    }

                                    if (Vars.gameData.state == State.Recording)
                                    {
                                        if (!Vars.isAdmin || Functions.GetAsyncKeyState(0x09) < 0) // GetAsyncKeyState only works with admin
                                        {
                                            if (Protocols.CheckHeroPlayed(frame.DesktopImage) && Vars.roundTimer.ElapsedMilliseconds >= Functions.GetTimeDeduction(getNextDeduction: true))
                                            {
                                                Protocols.CheckStats(frame.DesktopImage);
                                            }
                                        }
                                        else if (Vars.roundTimer.ElapsedMilliseconds >= Functions.GetTimeDeduction(getNextDeduction: true))
                                        {
                                            Protocols.CheckRoundCompleted(frame.DesktopImage);
                                        }
                                        Protocols.CheckMainMenu(frame.DesktopImage);
                                        Protocols.CheckFinalScore(frame.DesktopImage);
                                    }

                                    if (Vars.gameData.state == State.Finished && Vars.getInfoTimeout.ElapsedMilliseconds >= 500)
                                    {
                                        Protocols.CheckGameScore(frame.DesktopImage);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Functions.DebugMessage("Main Exception: " + e.ToString());
                                    Thread.Sleep(500);
                                }
                            }

                            Vars.frameTimer.Restart();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("CaptureDesktop() DO NOT RUN");
                    Thread.Sleep(1000);
                }
            }
        }
        private static Assembly LoadAssembly(string resource)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                byte[] assemblyData = new byte[stream.Length];

                stream.Read(assemblyData, 0, assemblyData.Length);
                return Assembly.Load(assemblyData);
            }
        }
    }
}