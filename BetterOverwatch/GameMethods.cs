using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using BetterOverwatch.Game;
using BetterOverwatch.Game.Objects;
using BetterOverwatch.Networking;
using BetterOverwatch.Properties;

namespace BetterOverwatch
{
    class GameMethods
    {
        public static bool IsOnCompetitiveScreen(Bitmap frame)
        {
            return Functions.CompareStrings(Functions.BitmapToText(frame, 73, 94, 622, 92, false, 110, Network.Maps, false), "COMPETITIVEPLAY") >= 80;
        }
        public static void ReadRoleRatings(Bitmap frame)
        {
            bool tankCheck = Functions.BitmapIsCertainColor(Functions.CaptureRegion(frame, 581, 600, 200, 1), 255, 255, 255);
            bool damageCheck = Functions.BitmapIsCertainColor(Functions.CaptureRegion(frame, 861, 600, 200, 1), 255, 255, 255);
            bool supportCheck = Functions.BitmapIsCertainColor(Functions.CaptureRegion(frame, 1140, 600, 200, 1), 255, 255, 255);
            int tankRating = 0, damageRating = 0, supportRating = 0;
            string debugString = "";

            if (tankCheck)
            {
                string tankRatingText = Functions.BitmapToText(frame, 662, 557, 61, 34, true, 110, Network.Ratings, true);

                if (tankRatingText.Length > 4)
                {
                    tankRatingText = tankRatingText.Substring(tankRatingText.Length - 4);
                }
                else if (tankRatingText.Length < 4)
                {
                    tankCheck = false;
                }
                if (tankCheck && int.TryParse(tankRatingText, out tankRating))
                {
                    debugString += $" Tank: {tankRatingText}";
                }
            }
            if (damageCheck)
            {
                string damageRatingText = Functions.BitmapToText(frame, 941, 557, 61, 34, true, 110, Network.Ratings, true);

                if (damageRatingText.Length > 4)
                {
                    damageRatingText = damageRatingText.Substring(damageRatingText.Length - 4);
                }
                else if (damageRatingText.Length < 4)
                {
                    damageCheck = false;
                }
                if (damageCheck && int.TryParse(damageRatingText, out damageRating))
                {
                    debugString += $" Damage: {damageRatingText}";
                }
            }
            if (supportCheck)
            {
                string supportRatingText = Functions.BitmapToText(frame, 1221, 557, 61, 34, true, 110, Network.Ratings, true);

                if (supportRatingText.Length > 4)
                {
                    supportRatingText = supportRatingText.Substring(supportRatingText.Length - 4);
                }
                else if (supportRatingText.Length < 4)
                {
                    supportCheck = false;
                }
                if (supportCheck && int.TryParse(supportRatingText, out supportRating))
                {
                    debugString += $" Support: {supportRatingText}";
                }
            }

            if ((tankCheck || damageCheck || supportCheck) &&
                (tankCheck && AppData.gameData.currentRatings.tank != tankRating ||
                 damageCheck && AppData.gameData.currentRatings.damage != damageRating ||
                supportCheck && AppData.gameData.currentRatings.support != supportRating ||
                AppData.gameData.state >= State.Recording))
            {
                if (tankCheck)
                {
                    AppData.gameData.currentRatings.tank = tankRating;
                    if (AppData.settings.outputToTextFiles)
                    {
                        try
                        {
                            File.WriteAllText("tank.txt", tankRating.ToString());
                            Functions.DebugMessage($"Updated tank.txt with '{tankRating}'");
                        }
                        catch (Exception e) { Functions.DebugMessage($"Failed to update tank.txt : {e.Message}"); }
                    }
                }
                if (damageCheck)
                {
                    AppData.gameData.currentRatings.damage = damageRating;
                    if (AppData.settings.outputToTextFiles)
                    {
                        try
                        {
                            File.WriteAllText("damage.txt", damageRating.ToString());
                            Functions.DebugMessage($"Updated damage.txt with '{damageRating}'");
                        }
                        catch (Exception e) { Functions.DebugMessage($"Failed to update damage.txt : {e.Message}"); }
                    }
                }
                if (supportCheck)
                {
                    AppData.gameData.currentRatings.support = supportRating;
                    if (AppData.settings.outputToTextFiles)
                    {
                        try
                        {
                            File.WriteAllText("support.txt", supportRating.ToString());
                            Functions.DebugMessage($"Updated support.txt with '{supportRating}'");
                        }
                        catch (Exception e) { Functions.DebugMessage($"Failed to update support.txt : {e.Message}"); }
                    }
                }
                AppData.successSound.Play();
                Functions.DebugMessage($"Recognized rating:{debugString}");

                if (AppData.gameData.state >= State.Recording)
                {
                    AppData.gameData.timer.Stop();
                    Server.CheckGameUpload();
                    AppData.gameData = new GameData(AppData.gameData.currentRatings);
                }
                else
                {
                    ScreenCaptureHandler.trayMenu.ChangeTray("Ready to record, enter a competitive game to begin", Resources.Icon_Active);
                }
            }
        }
        public static int[] ReadHeroStats(Bitmap frame, int[] statSettings)
        {
            int[] heroStats = { 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < statSettings.Length; i++)
            {
                if (statSettings[i] > 0)
                {
                    string heroStatText = Functions.BitmapToText(frame, Constants.heroStatsPositions[i][0], Constants.heroStatsPositions[i][1], statSettings[i] == 1 ? 40 : 80, 21, false, 110, Network.Numbers);

                    if (int.TryParse(heroStatText, out int heroStat))
                    {
                        heroStats[i] = heroStat;
                    }
                }
            }

            return heroStats;
        }
        public static void ReadStats(Bitmap frame)
        {
            if (AppData.statsTimer.Elapsed.TotalSeconds < 20) return;

            string eliminationsText = Functions.BitmapToText(frame, 129, 895, 40, 23, false, 110, Network.Numbers);
            if (eliminationsText.Equals(string.Empty)) return;
            string damageText = Functions.BitmapToText(frame, 129, 959, 80, 23, false, 110, Network.Numbers);
            if (damageText.Equals(string.Empty)) return;
            string objectiveKillsText = Functions.BitmapToText(frame, 379, 895, 40, 23, false, 110, Network.Numbers);
            if (objectiveKillsText.Equals(string.Empty)) return;
            string healingText = Functions.BitmapToText(frame, 379, 959, 80, 23, false, 110, Network.Numbers);
            if (healingText.Equals(string.Empty)) return;
            string deathsText = Functions.BitmapToText(frame, 629, 959, 40, 23, false, 110, Network.Numbers);
            if (deathsText.Equals(string.Empty)) return;

            if (int.TryParse(eliminationsText, out int eliminations) &&
                int.TryParse(damageText, out int damage) &&
                int.TryParse(objectiveKillsText, out int objectiveKills) &&
                int.TryParse(healingText, out int healing) &&
                int.TryParse(deathsText, out int deaths))
            {
                // integrity check, dont check hero stats or add any stats if everything is zero
                if (eliminations == 0 & objectiveKills == 0 & deaths == 0 & damage == 0 & healing == 0) return;
                int[] heroStats = { };

                for (int i = 0; i < Constants.heroList.Length; i++)
                {
                    if (Constants.heroList[i].name.Equals(AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].name))
                    {
                        heroStats = ReadHeroStats(frame, Constants.heroList[i].statSettings);
                        break;
                    }
                }
                AppData.gameData.stats.Add(new Stat((int)AppData.gameData.gameTimer.Elapsed.TotalSeconds, eliminations, damage, objectiveKills, healing, deaths, heroStats));
                AppData.statsTimer.Restart();
                Functions.DebugMessage($"Hero stats recorded after {(int)Math.Floor(AppData.gameData.gameTimer.Elapsed.TotalSeconds)} seconds");
            }
        }
        public static void ReadCompetitiveGameEntered(Bitmap frame)
        {
            string compText = Functions.BitmapToText(frame, 1354, 892, 323, 48, false, 120, 0, false, 255, 255, 0);

            if (!compText.Equals(string.Empty))
            {
                double percent = Functions.CompareStrings(compText, "COMPETITIVEPLAY");

                if (percent >= 70)
                {
                    if (AppData.gameData.state == State.Finished || AppData.gameData.state == State.WaitForUpload) // a game finished
                    {
                        Server.CheckGameUpload();
                    }
                    AppData.gameData = new GameData(AppData.gameData.currentRatings);
                    AppData.getInfoTimeout.Restart();
                    AppData.gameData.state = State.Ingame;
                    AppData.gameData.startRatings.tank = AppData.gameData.currentRatings.tank;
                    AppData.gameData.startRatings.damage = AppData.gameData.currentRatings.damage;
                    AppData.gameData.startRatings.support = AppData.gameData.currentRatings.support;

                    Functions.DebugMessage("Recognized competitive game");
                }
            }
        }
        public static void ReadMap(Bitmap frame)
        {
            if (AppData.gameData.map.Equals(string.Empty))
            {
                string mapText = Functions.BitmapToText(frame, 915, 945, 780, 85);

                if (!mapText.Equals(string.Empty))
                {
                    mapText = Functions.CheckMaps(mapText);

                    if (!mapText.Equals(string.Empty))
                    {
                        AppData.gameData.map = mapText;
                        AppData.getInfoTimeout.Restart();
                        Functions.DebugMessage($"Recognized map: '{mapText}'");
                    }
                }
            }
        }
        public static void ReadTeamsSkillRating(Bitmap frame)
        {
            if (AppData.gameData.team1Rating == 0)
            {
                string team1Rating = Functions.BitmapToText(frame, 545, 220, 245, 70, false, 90, Network.TeamSkillRating);
                team1Rating = Regex.Match(team1Rating, "[0-9]+").ToString();

                if (!team1Rating.Equals(string.Empty) && team1Rating.Length >= 4) // TEAM 1 SR
                {
                    team1Rating = team1Rating.Substring(team1Rating.Length - 4);

                    if (int.TryParse(team1Rating, out int rating) && rating > 999 && rating < 5000)
                    {
                        AppData.gameData.team1Rating = rating;
                        Functions.DebugMessage($"Recognized team 1 rating: '{team1Rating}'");
                    }
                }
            }
            if (AppData.gameData.team2Rating == 0)
            {
                string team2Rating = Functions.BitmapToText(frame, 1135, 220, 245, 70, false, 90, Network.TeamSkillRating);
                team2Rating = Regex.Match(team2Rating, "[0-9]+").ToString();

                if (!team2Rating.Equals(string.Empty) && team2Rating.Length >= 4) // TEAM 1 SR
                {
                    team2Rating = team2Rating.Substring(team2Rating.Length - 4);

                    if (int.TryParse(team2Rating, out int rating) && rating > 999 && rating < 5000)
                    {
                        AppData.gameData.team2Rating = rating;
                        Functions.DebugMessage($"Recognized team 2 rating: '{team2Rating}'");
                    }
                }
            }
        }
        public static void ReadMainMenu(Bitmap frame)
        {
            string menuText = Functions.BitmapToText(frame, 50, 234, 118, 58, false, 140);

            if (!menuText.Equals(string.Empty))
            {
                if (menuText.Equals("PLAY"))
                {
                    Functions.DebugMessage("Recognized main menu");
                    AppData.loopDelay = 250;
                    AppData.gameData.state = State.Finished;
                    AppData.gameData.timer.Stop();
                    AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time = (int)AppData.gameData.gameTimer.Elapsed.TotalSeconds;
                    ScreenCaptureHandler.trayMenu.ChangeTray("Visit play menu to upload last game", Resources.Icon_Wait);
                }
            }
        }
        public static bool ReadHeroPlayed(Bitmap frame)
        {
            string heroText = Functions.BitmapToText(frame, 955, 834, 170, 35, false, 200, Network.HeroNames);

            if (!heroText.Equals(string.Empty))
            {
                for (int h = 0; h < Constants.heroComparerList.Length; h++)
                {
                    double accuracy = Functions.CompareStrings(heroText, Constants.heroComparerList[h]);

                    if (accuracy >= 70)
                    {
                        if (AppData.gameData.heroesPlayed.Count > 0 &&
                            AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].name == Constants.heroList[h].name)
                        {
                            return true; // if playing the same hero, don't continue
                        }
                        if (AppData.gameData.heroesPlayed.Count > 0)
                        {
                            if ((AppData.gameData.state == State.RoundComplete || AppData.gameData.state == State.RoundBeginning) &&
                                AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time == 0)
                            {
                                AppData.gameData.heroesPlayed.RemoveAt(AppData.gameData.heroesPlayed.Count - 1);
                            }
                            else
                            {
                                AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time = (int)AppData.gameData.heroTimer.Elapsed.TotalSeconds;
                            }
                        }
                        AppData.gameData.heroTimer.Restart();
                        AppData.gameData.heroesPlayed.Add(new HeroPlayed(Constants.heroList[h].name, (int)AppData.gameData.gameTimer.Elapsed.TotalSeconds));
                        Functions.DebugMessage($"Now playing hero {Constants.heroList[h].name} at {AppData.gameData.gameTimer.Elapsed.TotalSeconds} seconds");
                        return true;
                    }
                }
            }
            return false;
        }
        public static void ReadRoundCompleted(Bitmap frame)
        {
            string roundCompletedText = Functions.BitmapToText(frame, 940, 160, 290, 80);

            if (roundCompletedText != string.Empty && Functions.CompareStrings(roundCompletedText, "COMPLETE") >= 70)
            {
                if (AppData.gameData.heroesPlayed.Count > 0)
                {
                    AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time = (int)AppData.gameData.heroTimer.Elapsed.TotalSeconds;
                }
                //Vars.gameData.heroesPlayed.Add(new HeroPlayed(Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].name, (int)Vars.gameData.gameTimer.Elapsed.TotalSeconds));
                AppData.gameData.state = State.RoundComplete;
                AppData.gameData.heroTimer.Stop();
                AppData.gameData.gameTimer.Stop();
                AppData.getInfoTimeout.Restart();
                Functions.DebugMessage($"Recognized round completed");
            }
        }
        public static bool ReadRoundStarted(Bitmap frame)
        {
            // hackfix: not actually reading the text, just checking the length
            string roundStartedText = Functions.BitmapToText(frame, 915, 70, 175, 13, true, 110, Network.Maps);

            if (roundStartedText.Length >= 10) AppData.gameData.objectiveTicks++;
            else if (roundStartedText.Length < 4) AppData.gameData.objectiveTicks = 0;

            return AppData.gameData.objectiveTicks >= 4;
        }
        public static void ReadFinalScore(Bitmap frame)
        {
            string finalScoreText = Functions.BitmapToText(frame, 870, 433, 180, 40);

            if (!finalScoreText.Equals(string.Empty))
            {
                if (Functions.CompareStrings(finalScoreText, "FIHNLSCORE") >= 40)
                {
                    Functions.DebugMessage("Recognized final score");
                    AppData.gameData.state = State.Finished;
                    AppData.gameData.timer.Stop();
                    AppData.gameData.gameTimer.Stop();
                    AppData.getInfoTimeout.Restart();
                    AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time = (int)AppData.gameData.heroTimer.Elapsed.TotalSeconds;
                    ScreenCaptureHandler.trayMenu.ChangeTray("Visit play menu to upload last game", Resources.Icon_Wait);
                }
            }
        }
        public static void ReadGameScore(Bitmap frame)
        {
            if (AppData.gameData.team1Score == 0 && AppData.gameData.team1Score == 0)
            {
                string scoreTextLeft = Functions.BitmapToText(frame, 800, 560, 95, 135, false, 45, Network.TeamSkillRating);
                string scoreTextRight = Functions.BitmapToText(frame, 1000, 560, 95, 135, false, 45, Network.TeamSkillRating);
                scoreTextLeft = Regex.Match(scoreTextLeft, "[0-9]+").ToString();
                scoreTextRight = Regex.Match(scoreTextRight, "[0-9]+").ToString();

                if (int.TryParse(scoreTextLeft, out int team1) &&
                    int.TryParse(scoreTextRight, out int team2) &&
                    team1 >= 0 && team1 <= 6 && team2 >= 0 && team2 <= 6)
                {
                    AppData.gameData.team1Score = team1;
                    AppData.gameData.team2Score = team2;
                    AppData.loopDelay = 250;
                    Functions.DebugMessage("Recognized team score Team 1:" + scoreTextLeft + " Team 2:" + scoreTextRight);
                    AppData.gameData.state = State.WaitForUpload;
                    AppData.getInfoTimeout.Stop();
                }
            }
        }
        public static void ReadPlayerNamesAndRank(Bitmap frame)
        {
            int playerNameX = 355, playerRankX = 733;

            for (int teams = 0; teams < 2; teams++)
            {
                for (int players = 0; players < 6; players++)
                {
                    string playerName = Functions.BitmapToText(frame, playerNameX, 325 + (players * 75), 260, 43, true, 110, Network.PlayerNames, false, 255, 255, 255, true, true);

                    if (playerName.Equals(string.Empty))
                    {
                        AppData.gameData.players.Clear();
                        return;
                    }
                    Bitmap rank = frame.Clone(new Rectangle(playerRankX, 331 + (players * 75), 33, 33), frame.PixelFormat);
                    byte[] backgroundColor = Functions.GetPixelAtPosition(rank, 0, 32);
                    Functions.AdjustColors(rank, 100, backgroundColor[0], backgroundColor[1], backgroundColor[2]); // fill rank with white
                    Functions.AdjustColors(rank, 100, backgroundColor[0], backgroundColor[1], backgroundColor[2], false); // fill backgroundColor with black
                    double[] resultRank = new double[2] { -1, -1 };

                    for (int r = 0; r < Constants.rankList.Length; r++)
                    {
                        double high = Functions.CompareTwoBitmaps(rank, Constants.rankList[r]);

                        if (high > resultRank[1])
                        {
                            resultRank[0] = r;
                            resultRank[1] = high;
                        }
                    }
                    AppData.gameData.players.Add(new Player(playerName, resultRank[0].ToString()));
                }
                playerNameX += 945;
                playerRankX += 422;
            }
            Functions.DebugMessage("Captured player list");
        }
        public static bool IsValidGame()
        {
            if (AppData.gameData.timer.Elapsed.TotalSeconds < 300 && !ScreenCaptureHandler.debug)
            {
                if (AppData.gameData.state >= State.Recording)
                {
                    Functions.DebugMessage($"Invalid game state={AppData.gameData.state} gameData.timer={AppData.gameData.timer.Elapsed.TotalSeconds}");
                    ScreenCaptureHandler.trayMenu.ChangeTray("Ready to record, enter a competitive game to begin", Resources.Icon_Active);
                }
                return false;
            }
            return true;
        }
    }
}
