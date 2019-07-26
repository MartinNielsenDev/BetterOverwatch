using System;
using System.Drawing;
using System.Threading;
using BetterOverwatch.DesktopDuplication;
using BetterOverwatch.Game;
using BetterOverwatch.Networking;
using BetterOverwatch.Properties;

namespace BetterOverwatch
{
    class ScreenCaptureHandler
    {
        public static Bitmap testImage = new Bitmap(@"C:\Users\Avoid\Desktop\ptr-test.png");
        public static bool captureScreen = false;
        private static DesktopDuplicator desktopDuplicator;
        public static TrayMenu trayMenu;
        public static void ScreenCapture()
        {
            AppData.statsTimer.Restart();
            try
            {
                desktopDuplicator = new DesktopDuplicator(0);
                AppData.frameTimer.Start();
            }
            catch (Exception e)
            {
                Functions.DebugMessage("Could not initialize desktopDuplication API - shutting down:" + e);
                Environment.Exit(0);
            }
            while (true)
            {
                // TODO: find a reliable way to check whether this page is present
                // ideas: read the big "competitive play" text or "tank, damage, support" text
                string tankRating = Functions.BitmapToText(testImage, 625, 594, 62, 32, false, 110, Network.TeamSkillRating, true);
                string damageRating = Functions.BitmapToText(testImage, 942, 594, 62, 32, false, 110, Network.TeamSkillRating, true);
                string supportRating = Functions.BitmapToText(testImage, 1260, 594, 62, 32, false, 110, Network.TeamSkillRating, true);

                Console.WriteLine($"Tank rating: {tankRating}");
                Console.WriteLine($"Damage rating: {damageRating}");
                Console.WriteLine($"Support rating: {supportRating}");
                continue;
                if (!captureScreen)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (!AppData.overwatchRunning)
                {
                    if (Functions.IsProcessOpen("Overwatch"))
                    {
                        if (AppData.gameData.currentRating > 0)
                        {
                            trayMenu.ChangeTray("Ready to record, enter a competitive game to begin", Resources.Icon_Active);
                        }
                        else
                        {
                            trayMenu.ChangeTray("Visit play menu to update your skill rating", Resources.Icon_Wait);
                        }
                        AppData.overwatchRunning = true;
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
                        AppData.overwatchRunning = false;
                        if (AppData.gameData.state == State.RoundComplete ||
                            AppData.gameData.state == State.Recording ||
                            AppData.gameData.state == State.Finished ||
                            AppData.gameData.state == State.WaitForUpload
                            )
                        {
                            Server.CheckGameUpload();
                            AppData.gameData = new GameData(AppData.gameData.currentRating);
                        }
                    }
                }
                if (!AppData.overwatchRunning)
                {
                    Thread.Sleep(500);
                    continue;
                }
                if (AppData.gameData.state == State.Recording && !Functions.ActiveWindowTitle().Equals("Overwatch"))
                {
                    Thread.Sleep(500);
                    continue;
                }
                else
                {
                    Thread.Sleep(50);
                }
                if (AppData.frameTimer.ElapsedMilliseconds >= AppData.loopDelay)
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
                            if (AppData.gameData.state != State.Ingame)
                            {
                                string quickPlayText = Functions.BitmapToText(frame.DesktopImage, 476, 644, 80, 40, false, 140, Network.Maps, true);

                                if (Functions.CompareStrings(quickPlayText, "PLHY") >= 100)
                                {
                                    Thread.Sleep(150);
                                    GameMethods.ReadPlayMenu(frame.DesktopImage);
                                }
                            }
                            if (AppData.gameData.state == State.Idle || AppData.gameData.state == State.Finished || AppData.gameData.state == State.WaitForUpload)
                            {
                                GameMethods.ReadCompetitiveGameEntered(frame.DesktopImage);
                            }
                            if (AppData.gameData.state == State.Ingame)
                            {
                                if (AppData.getInfoTimeout.ElapsedMilliseconds > 5000)
                                {
                                    AppData.gameData.timer.Reset();
                                    AppData.getInfoTimeout.Reset();
                                    AppData.gameData.state = State.Idle;
                                    Functions.DebugMessage("Failed to find game");
                                }
                                else if (AppData.getInfoTimeout.ElapsedMilliseconds > 1500 || !AppData.gameData.map.Equals(string.Empty))
                                {
                                    if (AppData.gameData.map.Equals(string.Empty))
                                    {
                                        GameMethods.ReadMap(frame.DesktopImage);
                                    }
                                    else
                                    {
                                        GameMethods.ReadTeamsSkillRating(frame.DesktopImage);

                                        if (AppData.gameData.playerListImage == null)
                                        {
                                            try
                                            {
                                                frame = desktopDuplicator.GetLatestFrame();
                                                AppData.gameData.playerListImage = new Bitmap(Functions.CaptureRegion(frame.DesktopImage, 0, 110, 1920, 700));
                                                GameMethods.ReadPlayerNamesAndRank(frame.DesktopImage);
                                            }
                                            catch { }
                                        }
                                        AppData.loopDelay = 500;
                                        AppData.gameData.state = State.RoundComplete;
                                        AppData.gameData.timer.Start();
                                        AppData.statsTimer.Restart();
                                        AppData.getInfoTimeout.Start();
                                        trayMenu.ChangeTray("Recording... visit the main menu after the game", Resources.Icon_Record);
                                    }
                                }
                            }
                            if (AppData.gameData.state == State.Recording)
                            {
                                if (AppData.gameData.tabPressed && AppData.gameData.tabTimer.ElapsedMilliseconds > 250)
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
                            if (AppData.gameData.state == State.RoundComplete)
                            {
                                if (AppData.gameData.tabPressed && AppData.gameData.tabTimer.ElapsedMilliseconds > 250)
                                {
                                    GameMethods.ReadHeroPlayed(frame.DesktopImage);
                                }
                                else if (GameMethods.ReadRoundStarted(frame.DesktopImage) || AppData.getInfoTimeout.Elapsed.TotalSeconds > 80)
                                {
                                    AppData.getInfoTimeout.Restart();
                                    AppData.gameData.state = State.RoundBeginning;
                                }
                                GameMethods.ReadMainMenu(frame.DesktopImage);
                            }
                            if (AppData.gameData.state == State.RoundBeginning)
                            {
                                if (AppData.getInfoTimeout.Elapsed.TotalSeconds >= 40 || AppData.getInfoTimeout.Elapsed.TotalSeconds >= 30 && AppData.gameData.IsKoth())
                                {
                                    AppData.gameData.gameTimer.Start();
                                    AppData.gameData.heroTimer.Start();

                                    if (AppData.gameData.heroesPlayed.Count > 0 && AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time == 0)
                                    {
                                        AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].startTime = (int)AppData.gameData.gameTimer.Elapsed.TotalSeconds;
                                    }
                                    AppData.gameData.state = State.Recording;
                                }
                                else if (AppData.gameData.tabPressed && AppData.gameData.tabTimer.ElapsedMilliseconds > 250/*Functions.GetAsyncKeyState(0x09) < 0*/)
                                {
                                    GameMethods.ReadHeroPlayed(frame.DesktopImage);
                                }
                            }
                            if (AppData.gameData.state == State.Finished && AppData.getInfoTimeout.ElapsedMilliseconds >= 500)
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
                    AppData.frameTimer.Restart();
                }
            }
        }
    }
}
