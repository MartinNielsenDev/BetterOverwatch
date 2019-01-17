using System;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Security.Principal;
using System.Runtime.InteropServices;
using DesktopDuplication;

namespace OverwatchTracker
{
    class Program
    {
        [DllImport("User32.dll")]
        public static extern short GetAsyncKeyState(int vKey);
        public static ContextMenu contextMenu;
        public static Thread uploaderThread;
        private static Mutex mutex = new Mutex(true, "74bf6260-c133-4d69-ad9c-efc607887c97");
        private static DesktopDuplicator desktopDuplicator;
        [STAThread]
        static void Main()
        {
            Functions.DebugMessage("Starting Overwatch Tracker version " + Vars.version);
            
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                Functions.DebugMessage("Overwatch Tracker is already running...");
                return;
            }
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler((s, a) =>
            {
                if (a.Name.Contains("Newtonsoft.Json,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchTracker.dlls.Newtonsoft.Json.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (a.Name.Contains("AForge.Imaging,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchTracker.dlls.AForge.Imaging.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (a.Name.Contains("SharpDX.Direct3D11,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchTracker.dlls.SharpDX.Direct3D11.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (a.Name.Contains("SharpDX.DXGI,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchTracker.dlls.SharpDX.DXGI.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                if (a.Name.Contains("SharpDX,"))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OverwatchTracker.dlls.SharpDX.dll"))
                    {
                        byte[] assemblyData = new byte[stream.Length];

                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                }
                return null;
            });
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls;
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    Vars.isAdmin = new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                }
                if (!Server.newestVersion())
                {
                    return;
                }
                Directory.CreateDirectory(Vars.configPath);
                if (!Vars.isAdmin)
                {
                    MessageBox.Show("Overwatch Tracker was not run as administrator, this will result in your games being uploaded as PLAYER-0000 however the application will still function.\n\n\nIf you wish for Overwatch Tracker to also track your battletag, restart the app as administrator", "Missing privileges", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Vars.settings = new Settings();
                Settings.Load();
                Settings.Save();
                // load settings first, create GameData() object, then
                Vars.gameData = new GameData();
                contextMenu = new ContextMenu { TopMost = true };
                
                if (!Settings.FetchUserInfo())
                {
                    return;
                }
                /*
                Vars.gameData.map = "Hanamura";
                Vars.gameData.currentSkillRating = "3025";
                Vars.gameData.startsr = "3000";
                Server.uploadGame(Vars.gameData.GetData());
                */
                Thread captureDesktopThread = new Thread(captureDesktop);
                captureDesktopThread.IsBackground = true;
                captureDesktopThread.Start();
                Server.autoUpdaterTimer.Start();
                Functions.DebugMessage("> Success - Overwatch Tracker started without fail");
            }
            catch (Exception e)
            {
                MessageBox.Show("startUp error: " + e.ToString() + "\r\n\r\nReport this on the discord server");
                Environment.Exit(0);
            }
            Application.Run(contextMenu);
        }
        public static void captureDesktop()
        {
            Vars.statsTimer.Restart();
            try
            {
                desktopDuplicator = new DesktopDuplicator(0);
                Vars.frameTimer.Start();
            }
            catch (Exception err)
            {
                Functions.DebugMessage("Could not initialize desktopDuplication API - shutting down:" + err.ToString());
                Environment.Exit(0);
            }
            while (true)
            {
                if (!Functions.activeWindowTitle().Equals("Overwatch"))
                {
                    if (!Vars.overwatchRunning)
                    {
                        if (Functions.isProcessOpen("Overwatch"))
                        {
                            contextMenu.trayIcon.Text = "Visit play menu to update your skill rating";
                            contextMenu.trayIcon.Icon = Properties.Resources.IconVisitMenu;
                            Vars.overwatchRunning = true;
                        }
                        else
                        {
                            Server.autoUpdater();
                        }
                    }
                    else
                    {
                        if (!Functions.isProcessOpen("Overwatch"))
                        {
                            contextMenu.trayIcon.Text = "Waiting for Overwatch, idle...";
                            contextMenu.trayIcon.Icon = Properties.Resources.Idle;
                            Vars.overwatchRunning = false;
                        }
                    }
                    Thread.Sleep(500);
                }
                else
                {
                    Thread.Sleep(50);
                    contextMenu.currentGame.MenuItems[0].Text = "Game elapsed: " + Functions.secondsToMinutes((int)Math.Floor(Convert.ToDouble(Vars.gameTimer.ElapsedMilliseconds / 1000)));

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
                                Protocols.checkMainMenu(frame.DesktopImage);

                                if (Vars.gameData.gameState != Vars.STATUS_INGAME)
                                {
                                    string quickPlayText = Functions.bitmapToText(frame.DesktopImage, 476, 644, 80, 40, contrastFirst: false, radius: 140, network: 0, invertColors: true);

                                    if (Functions.compareStrings(quickPlayText, "PLRY") >= 70)
                                    {
                                        Protocols.checkPlayMenu(frame.DesktopImage);
                                    }
                                }

                                if (Vars.gameData.gameState == Vars.STATUS_IDLE || Vars.gameData.gameState == Vars.STATUS_FINISHED || Vars.gameData.gameState == Vars.STATUS_WAITFORUPLOAD)
                                {
                                    Protocols.checkCompetitiveGameEntered(frame.DesktopImage);
                                }

                                if (Vars.gameData.gameState == Vars.STATUS_INGAME)
                                {
                                    Protocols.checkMap(frame.DesktopImage);
                                    Protocols.checkTeamsSkillRating(frame.DesktopImage);

                                    if (!Vars.gameData.team1sr.Equals(String.Empty) &&
                                        !Vars.gameData.team2sr.Equals(String.Empty) &&
                                        !Vars.gameData.map.Equals(String.Empty) ||
                                        !Vars.gameData.map.Equals(String.Empty) && Vars.getInfoTimeout.ElapsedMilliseconds >= 4000)
                                    {
                                        if (Vars.gameData.playerlistimage == null)
                                        {
                                            Functions.DebugMessage("Taking screenshot in 1.5 seconds...");
                                            Thread.Sleep(1500);
                                            Vars.gameData.playerlistimage = new Bitmap(Functions.captureRegion(frame.DesktopImage, 0, 110, 1920, 700));
                                            Functions.DebugMessage("*CLICK* done");
                                        }
                                        Vars.loopDelay = 500;
                                        Vars.gameData.gameState = Vars.STATUS_RECORDING;
                                        contextMenu.trayIcon.Text = "Recording... visit the main menu after the game";
                                        contextMenu.trayIcon.Icon = Properties.Resources.IconRecord;
                                        Vars.statsTimer.Restart();
                                        Vars.getInfoTimeout.Stop();

                                    }
                                    else if (Vars.getInfoTimeout.ElapsedMilliseconds >= 4500)
                                    {
                                        Vars.roundTimer.Stop();
                                        Vars.gameTimer.Stop();
                                        Vars.getInfoTimeout.Stop();
                                        Vars.gameData.gameState = Vars.STATUS_IDLE;
                                        Functions.DebugMessage("Failed to find game");
                                    }
                                }

                                if (Vars.gameData.gameState == Vars.STATUS_RECORDING)
                                {
                                    if (GetAsyncKeyState(0x09) < 0)
                                    {
                                        Protocols.checkHeroPlayed(frame.DesktopImage);
                                        if (Vars.roundTimer.ElapsedMilliseconds >= Functions.getTimeDeduction(getNextDeduction: true))
                                        {
                                            Protocols.checkStats(frame.DesktopImage);
                                        }
                                    }
                                    else if (Vars.roundTimer.ElapsedMilliseconds >= Functions.getTimeDeduction(getNextDeduction: true))
                                    {
                                        Protocols.checkRoundCompleted(frame.DesktopImage);
                                    }
                                    Protocols.checkMainMenu(frame.DesktopImage);
                                    Protocols.checkFinalScore(frame.DesktopImage);
                                }

                                if (Vars.gameData.gameState == Vars.STATUS_FINISHED)
                                {
                                    Protocols.checkGameScore(frame.DesktopImage);
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
        }
    }
}
