using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using BetterOverwatch.DataObjects;
using BetterOverwatch.DesktopDuplication;
using BetterOverwatch.Forms;
using BetterOverwatch.Game;
using BetterOverwatch.Networking;
using BetterOverwatch.Properties;

namespace BetterOverwatch
{
    internal class Program
    {
        public static bool captureDesktop = false;
        public static TrayMenu trayMenu;
        public static AuthorizeForm authorizeForm;
        public static AdminPromptForm adminPromptForm;
        private static DesktopDuplicator desktopDuplicator;
        private static KeyboardHook keyboardHook;
        private static readonly Mutex mutex = new Mutex(true, "74bf6260-c133-4d69-ad9c-efc607887c97");
        [STAThread]
        private static void Main()
        {
            Vars.initalize = new Initalize(
                "1.3.1",
                "betteroverwatch.com",
                "https://api.github.com/repos/MartinNielsenDev/OverwatchTracker/releases/latest");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Functions.DebugMessage("Starting Better Overwatch version " + Vars.initalize.Version);

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Better Overwatch is already running\r\n\r\nYou must close other instances of Better Overwatch if you want to open this one", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            AppDomain.CurrentDomain.AssemblyResolve += (s, assembly) =>
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
            };

            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    Vars.isAdmin = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                }
                if (!Server.CheckNewestVersion()) return;

                Directory.CreateDirectory(Vars.configPath);
                Vars.settings = new Settings();
                Settings.Load();
                Vars.gameData = new GameData();
                trayMenu = new TrayMenu();
                Server.autoUpdaterTimer.Start();
                keyboardHook = new KeyboardHook(true);
                keyboardHook.KeyDown += TABPressed;
                keyboardHook.KeyUp += TABReleased;
                Functions.DebugMessage("Better Overwatch started");
            }
            catch (Exception e)
            {
                MessageBox.Show("Startup error: " + e + "\r\n\r\nReport this on the discord server");
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
                Functions.DebugMessage("Could not initialize desktopDuplication API - shutting down:" + e);
                Environment.Exit(0);
            }
            while (true)
            {
                if (!captureDesktop)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (!Functions.ActiveWindowTitle().Equals("Overwatch"))
                {
                    if (!Vars.overwatchRunning)
                    {
                        if (Functions.IsProcessOpen("Overwatch"))
                        {
                            if (Vars.gameData.currentRating > 0)
                            {
                                trayMenu.ChangeTray("Ready to record, enter a competitive game to begin", Resources.Icon_Active);
                            }
                            else
                            {
                                trayMenu.ChangeTray("Visit play menu to update your skill rating", Resources.Icon_Wait);
                            }
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
                            trayMenu.ChangeTray("Waiting for Overwatch, idle...", Resources.Icon);
                            Vars.overwatchRunning = false;
                            if (Vars.gameData.state == State.Recording ||
                                Vars.gameData.state == State.Finished ||
                                Vars.gameData.state == State.WaitForUpload
                                )
                            {
                                Server.CheckGameUpload();
                                Vars.gameData = new GameData(Vars.gameData.currentRating);
                            }
                        }
                    }
                    Thread.Sleep(500);
                }
                else
                {
                    Thread.Sleep(50);

                    if (Vars.frameTimer.ElapsedMilliseconds >= Vars.loopDelay)
                    {
                        DesktopFrame frame;

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
                                    string quickPlayText = Functions.BitmapToText(frame.DesktopImage, 476, 644, 80, 40, false, 140, Network.Maps, true);

                                    if (Functions.CompareStrings(quickPlayText, "PLHY") >= 100)
                                    {
                                        Thread.Sleep(150);
                                        GameMethods.ReadPlayMenu(frame.DesktopImage);
                                    }
                                }
                                if (Vars.gameData.state == State.Idle || Vars.gameData.state == State.Finished || Vars.gameData.state == State.WaitForUpload)
                                {
                                    GameMethods.ReadCompetitiveGameEntered(frame.DesktopImage);
                                }
                                if (Vars.gameData.state == State.Ingame)
                                {
                                    if (Vars.getInfoTimeout.Elapsed.TotalSeconds > 2)
                                    {
                                        GameMethods.ReadMap(frame.DesktopImage);
                                        GameMethods.ReadTeamsSkillRating(frame.DesktopImage);

                                        if (!Vars.gameData.map.Equals(string.Empty))
                                        {
                                            if (Vars.gameData.playerListImage == null)
                                            {
                                                try
                                                {
                                                    frame = desktopDuplicator.GetLatestFrame();
                                                    Vars.gameData.playerListImage = new Bitmap(Functions.CaptureRegion(frame.DesktopImage, 0, 110, 1920, 700));
                                                    GameMethods.ReadPlayerNamesAndRank(frame.DesktopImage);
                                                }
                                                catch { }
                                            }
                                            Vars.loopDelay = 500;
                                            Vars.gameData.state = State.RoundComplete;
                                            Vars.gameData.timer.Start();
                                            Vars.statsTimer.Restart();
                                            Vars.getInfoTimeout.Start();
                                            trayMenu.ChangeTray("Recording... visit the main menu after the game", Resources.Icon_Record);
                                        }
                                    }
                                    else if (Vars.getInfoTimeout.Elapsed.TotalSeconds > 5)
                                    {
                                        Vars.gameData.timer.Reset();
                                        Vars.getInfoTimeout.Reset();
                                        Vars.gameData.state = State.Idle;
                                        Functions.DebugMessage("Failed to find game");
                                    }
                                }
                                if (Vars.gameData.state == State.Recording)
                                {
                                    if (Vars.gameData.tabPressed && Vars.gameData.tabTimer.ElapsedMilliseconds > 250)
                                    {
                                        if (GameMethods.ReadHeroPlayed(frame.DesktopImage))
                                        {
                                            GameMethods.ReadStats(frame.DesktopImage);
                                        }
                                    }
                                    GameMethods.ReadRoundCompleted(frame.DesktopImage);
                                    GameMethods.ReadMainMenu(frame.DesktopImage);
                                    GameMethods.ReadFinalScore(frame.DesktopImage);
                                }
                                if (Vars.gameData.state == State.RoundComplete)
                                {
                                    if (Vars.gameData.tabPressed && Vars.gameData.tabTimer.ElapsedMilliseconds > 250)
                                    {
                                        GameMethods.ReadHeroPlayed(frame.DesktopImage);
                                    }
                                    else if (GameMethods.ReadRoundStarted(frame.DesktopImage) || Vars.getInfoTimeout.Elapsed.TotalSeconds > 80)
                                    {
                                        Vars.getInfoTimeout.Restart();
                                        Vars.gameData.state = State.RoundBeginning;
                                    }
                                }
                                if (Vars.gameData.state == State.RoundBeginning)
                                {
                                    if (Vars.getInfoTimeout.Elapsed.TotalSeconds >= 40 || Vars.getInfoTimeout.Elapsed.TotalSeconds >= 30 && Vars.gameData.IsKoth())
                                    {
                                        Vars.gameData.gameTimer.Start();
                                        Vars.gameData.heroTimer.Start();

                                        if (Vars.gameData.heroesPlayed.Count > 0 && Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].time == 0)
                                        {
                                            Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].startTime = (int)Vars.gameData.gameTimer.Elapsed.TotalSeconds;
                                        }
                                        Vars.gameData.state = State.Recording;
                                    }
                                    else if (Vars.gameData.tabPressed && Vars.gameData.tabTimer.ElapsedMilliseconds > 250/*Functions.GetAsyncKeyState(0x09) < 0*/)
                                    {
                                        GameMethods.ReadHeroPlayed(frame.DesktopImage);
                                    }
                                }
                                if (Vars.gameData.state == State.Finished && Vars.getInfoTimeout.ElapsedMilliseconds >= 500)
                                {
                                    GameMethods.ReadGameScore(frame.DesktopImage);
                                }
                            }
                            catch (Exception e)
                            {
                                Functions.DebugMessage("Main Exception: " + e);
                                Thread.Sleep(500);
                            }
                        }
                        Vars.frameTimer.Restart();
                    }
                }
            }
        }
        private static void TABPressed(Keys key, bool Shift, bool Ctrl, bool Alt)
        {
            if (key == Keys.Tab)
            {
                Vars.gameData.tabTimer.Start();
                Vars.gameData.tabPressed = true;
            }
        }
        private static void TABReleased(Keys key, bool Shift, bool Ctrl, bool Alt)
        {
            if (key == Keys.Tab)
            {
                Vars.gameData.tabTimer.Reset();
                Vars.gameData.tabPressed = false;
            }
        }
        private static Assembly LoadAssembly(string resource)
        {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
            {
                if (stream != null)
                {
                    byte[] assemblyData = new byte[stream.Length];

                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            }
            return null;
        }
    }
}