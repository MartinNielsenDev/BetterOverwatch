﻿using System;
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
#if DEBUG
        internal static bool debug = true;
#else
        internal static bool debug = false;
#endif
        internal static bool captureScreen = false;
        private static DesktopDuplicator desktopDuplicator;
        internal static TrayMenu trayMenu;
        internal static void ScreenCapture()
        {
            AppData.statsTimer.Restart();
            try
            {
                desktopDuplicator = new DesktopDuplicator(0);
            }
            catch (Exception e)
            {
                Functions.DebugMessage("Could not initialize desktopDuplication API - shutting down:" + e);
                Environment.Exit(0);
            }
            while (true)
            {
                if (!captureScreen)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                if (!AppData.overwatchRunning && !debug)
                {
                    if (Functions.IsProcessOpen("Overwatch"))
                    {
                        if (AppData.gameData.currentRatings.AverageRating() > 0)
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
                    if (!Functions.IsProcessOpen("Overwatch") && !debug)
                    {
                        trayMenu.ChangeTray("Waiting for Overwatch, idle...", Resources.Icon);
                        AppData.overwatchRunning = false;
                        if (AppData.gameData.state == State.RoundComplete ||
                            AppData.gameData.state == State.Record ||
                            AppData.gameData.state == State.Finished ||
                            AppData.gameData.state == State.Upload
                            )
                        {
                            Server.CheckGameUpload();
                            AppData.gameData = new GameData(AppData.gameData.currentRatings);
                        }
                    }
                }
                if ((!AppData.overwatchRunning || AppData.gameData.state == State.Record && !Functions.ActiveWindowTitle().Equals("Overwatch")) && !debug)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                else
                {
                    Thread.Sleep(AppData.loopDelay);
                }
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
                            if (GameMethods.IsOnCompetitiveScreen(frame.DesktopImage) || true)
                            {
                                if (AppData.ratingsTimer.ElapsedMilliseconds >= 500)
                                {
                                    GameMethods.ReadRoleRatings(frame.DesktopImage);
                                }
                                else
                                {
                                    AppData.ratingsTimer.Start();
                                }
                            }
                            else
                            {
                                AppData.ratingsTimer.Reset();
                            }
                        }
                        if (AppData.gameData.state == State.Idle || AppData.gameData.state == State.Finished || AppData.gameData.state == State.Upload)
                        {
                            GameMethods.ReadCompetitiveGameEntered(frame.DesktopImage);
                        }
                        if (AppData.gameData.state == State.Ingame)
                        {
                            if (AppData.infoTimer.ElapsedMilliseconds > 5000)
                            {
                                AppData.gameData.timer.Reset();
                                AppData.infoTimer.Reset();
                                AppData.gameData.state = State.Idle;
                                Functions.DebugMessage("Failed to find game");
                            }
                            else if (AppData.infoTimer.ElapsedMilliseconds > 1500 || !AppData.gameData.map.Equals(string.Empty))
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
                                            AppData.gameData.playerListImage = new Bitmap(BitmapFunctions.CropImage(frame.DesktopImage, Rectangles.PlayerListImage));
                                            GameMethods.ReadPlayerNamesAndRank(frame.DesktopImage);
                                        }
                                        catch { }
                                    }
                                    AppData.loopDelay = 500;
                                    AppData.gameData.state = State.RoundComplete;
                                    AppData.gameData.timer.Start();
                                    AppData.ratingsTimer.Reset();
                                    AppData.statsTimer.Restart();
                                    AppData.infoTimer.Restart();
                                    trayMenu.ChangeTray("Recording... visit the main menu after the game", Resources.Icon_Record);
                                }
                            }
                        }
                        if (AppData.gameData.state == State.Record)
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
                            else if (GameMethods.ReadRoundStarted(frame.DesktopImage) || AppData.infoTimer.Elapsed.TotalSeconds > 80)
                            {
                                Functions.DebugMessage("Waiting for doors to open...");
                                AppData.infoTimer.Restart();
                                AppData.gameData.state = State.RoundStart;
                            }
                            GameMethods.ReadMainMenu(frame.DesktopImage);
                        }
                        if (AppData.gameData.state == State.RoundStart)
                        {
                            if (AppData.infoTimer.Elapsed.TotalSeconds >= 40 || AppData.infoTimer.Elapsed.TotalSeconds >= 30 && AppData.gameData.IsKoth())
                            {
                                AppData.gameData.gameTimer.Start();
                                AppData.gameData.heroTimer.Start();

                                if (AppData.gameData.heroesPlayed.Count > 0 && AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time == 0)
                                {
                                    AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].startTime = (int)AppData.gameData.gameTimer.Elapsed.TotalSeconds;
                                }
                                AppData.gameData.state = State.Record;
                                int roundedSecs = AppData.gameData.gameTimer.Elapsed.TotalSeconds < 2000 ? (int)Math.Floor(AppData.gameData.gameTimer.Elapsed.TotalSeconds) : 0;
                                Functions.DebugMessage($"Round started after {roundedSecs} seconds, goodluck!");
                            }
                            else if (AppData.gameData.tabPressed && AppData.gameData.tabTimer.ElapsedMilliseconds > 250/*Functions.GetAsyncKeyState(0x09) < 0*/)
                            {
                                GameMethods.ReadHeroPlayed(frame.DesktopImage);
                            }
                        }
                        if (AppData.gameData.state == State.Finished && AppData.infoTimer.ElapsedMilliseconds >= 500)
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
            }
        }
    }
}