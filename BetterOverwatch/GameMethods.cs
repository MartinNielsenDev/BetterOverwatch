using System;
using System.Drawing;
using System.Text.RegularExpressions;
using BetterOverwatch.Game;
using BetterOverwatch.Game.Objects;
using BetterOverwatch.Networking;
using BetterOverwatch.Properties;

namespace BetterOverwatch
{
    class GameMethods
    {
        public static void ReadPlayMenu(Bitmap frame)
        {
            string ratingText = Functions.BitmapToText(frame, 1100, 444, 100, 40, true, 110, Network.Numbers); // GROUP CHECK
            ratingText = Regex.Match(ratingText, "[0-9]+").ToString();

            if (ratingText.Length < 4)
            {
                ratingText = Functions.BitmapToText(frame, 1100, 504, 100, 32, true, 110, Network.Numbers); // SOLO CHECK
                ratingText = Regex.Match(ratingText, "[0-9]+").ToString();
            }
            if (ratingText.Length > 4)
            {
                ratingText = ratingText.Substring(ratingText.Length - 4);
            }

            if (int.TryParse(ratingText, out int rating) && rating > 999 && rating < 5000)
            {
                if (Vars.gameData.currentRating != rating || Vars.gameData.state >= State.Finished)
                {
                    Vars.successSound.Play();
                    Functions.DebugMessage("Recognized rating: '" + ratingText + "'");
                    Program.trayMenu.ChangeTray("Ready to record, enter a competitive game to begin", Resources.Icon_Active);
                }
                Vars.gameData.timer.Stop();
                Vars.gameData.currentRating = rating;

                if (Vars.gameData.state == State.Recording ||
                    Vars.gameData.state == State.Finished ||
                    Vars.gameData.state == State.WaitForUpload)
                {
                    Server.CheckGameUpload();
                    Vars.gameData = new GameData(Vars.gameData.currentRating);
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
            if (Vars.statsTimer.Elapsed.TotalSeconds < 20) return;

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
                    if (Constants.heroList[i].name.Equals(Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].name))
                    {
                        heroStats = ReadHeroStats(frame, Constants.heroList[i].statSettings);
                        break;
                    }
                }
                //frame.Save(@"C:\testData\" + eliminations + "_" + Guid.NewGuid() + ".png"); // test
                Vars.gameData.stats.Add(new Stat((int)Vars.gameData.gameTimer.Elapsed.TotalSeconds, eliminations, damage, objectiveKills, healing, deaths, heroStats));
                Vars.statsTimer.Restart();
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
                    if (Vars.gameData.state == State.Finished || Vars.gameData.state == State.WaitForUpload) // a game finished
                    {
                        Server.CheckGameUpload();
                    }
                    Vars.gameData = new GameData(Vars.gameData.currentRating);
                    Vars.getInfoTimeout.Restart();
                    Vars.gameData.state = State.Ingame;
                    Vars.gameData.startRating = Vars.gameData.currentRating;

                    Functions.DebugMessage("Recognized competitive game");
                }
            }
        }
        public static void ReadMap(Bitmap frame)
        {
            if (Vars.gameData.map.Equals(string.Empty))
            {
                string mapText = Functions.BitmapToText(frame, 915, 945, 780, 85);

                if (!mapText.Equals(string.Empty))
                {
                    mapText = Functions.CheckMaps(mapText);

                    if (!mapText.Equals(string.Empty))
                    {
                        Vars.gameData.map = mapText;
                        Functions.DebugMessage("Recognized map: '" + mapText + "'");
                    }
                }
            }
        }
        public static void ReadTeamsSkillRating(Bitmap frame)
        {
            if (Vars.gameData.team1Rating == 0)
            {
                string team1Rating = Functions.BitmapToText(frame, 545, 220, 245, 70, false, 90, Network.TeamSkillRating);
                team1Rating = Regex.Match(team1Rating, "[0-9]+").ToString();

                if (!team1Rating.Equals(string.Empty) && team1Rating.Length >= 4) // TEAM 1 SR
                {
                    team1Rating = team1Rating.Substring(team1Rating.Length - 4);

                    if (int.TryParse(team1Rating, out int rating) && rating > 999 && rating < 5000)
                    {
                        Vars.gameData.team1Rating = rating;
                        Functions.DebugMessage("Recognized team 1 SR: '" + team1Rating + "'");
                    }
                }
            }
            if (Vars.gameData.team2Rating == 0)
            {
                string team2Rating = Functions.BitmapToText(frame, 1135, 220, 245, 70, false, 90, Network.TeamSkillRating);
                team2Rating = Regex.Match(team2Rating, "[0-9]+").ToString();

                if (!team2Rating.Equals(string.Empty) && team2Rating.Length >= 4) // TEAM 1 SR
                {
                    team2Rating = team2Rating.Substring(team2Rating.Length - 4);

                    if (int.TryParse(team2Rating, out int rating) && rating > 999 && rating < 5000)
                    {
                        Vars.gameData.team2Rating = rating;
                        Functions.DebugMessage("Recognized team 2 SR: '" + team2Rating + "'");
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
                    Vars.loopDelay = 250;
                    Vars.gameData.state = State.Finished;
                    Vars.gameData.timer.Stop();
                    Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].time = (int)Vars.gameData.gameTimer.Elapsed.TotalSeconds;
                    Program.trayMenu.ChangeTray("Visit play menu to upload last game", Resources.Icon_Wait);
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
                        if (Vars.gameData.heroesPlayed.Count > 0 &&
                            Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].name == Constants.heroList[h].name)
                        {
                            return true; // if playing the same hero, don't continue
                        }
                        if (Vars.gameData.heroesPlayed.Count > 0)
                        {
                            if ((Vars.gameData.state == State.RoundComplete || Vars.gameData.state == State.RoundBeginning) &&
                                Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].time == 0)
                            {
                                Vars.gameData.heroesPlayed.RemoveAt(Vars.gameData.heroesPlayed.Count - 1);
                            }
                            else
                            {
                                Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].time = (int)Vars.gameData.heroTimer.Elapsed.TotalSeconds;
                            }
                        }
                        Vars.gameData.heroTimer.Restart();
                        Vars.gameData.heroesPlayed.Add(new HeroPlayed(Constants.heroList[h].name, (int)Vars.gameData.gameTimer.Elapsed.TotalSeconds));
                        return true;
                    }
                }
            }
            return false;
        }
        public static void ReadRoundCompleted(Bitmap frame)
        {
            string roundCompletedText = Functions.BitmapToText(frame, 940, 160, 290, 76);

            if (!roundCompletedText.Equals(string.Empty))
            {
                if (Functions.CompareStrings(roundCompletedText, "COMPLETED") >= 70)
                {
                    Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].time = (int)Vars.gameData.heroTimer.Elapsed.TotalSeconds;
                    //Vars.gameData.heroesPlayed.Add(new HeroPlayed(Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].name, (int)Vars.gameData.gameTimer.Elapsed.TotalSeconds));
                    Vars.gameData.state = State.RoundComplete;
                    Vars.gameData.heroTimer.Stop();
                    Vars.gameData.gameTimer.Stop();
                    Vars.getInfoTimeout.Restart();
                    Functions.DebugMessage($"Recognized round completed");
                }
            }
        }
        public static bool ReadRoundStarted(Bitmap frame)
        {
            // hackfix: not actually reading the text, just checking the length
            string roundStartedText = Functions.BitmapToText(frame, 915, 70, 175, 13, true, 110, Network.Maps);

            if (roundStartedText.Length >= 10) Vars.gameData.objectiveTicks++;
            else if (roundStartedText.Length < 4) Vars.gameData.objectiveTicks = 0;

            return Vars.gameData.objectiveTicks >= 4;
        }
        public static void ReadFinalScore(Bitmap frame)
        {
            string finalScoreText = Functions.BitmapToText(frame, 870, 433, 180, 40);

            if (!finalScoreText.Equals(string.Empty))
            {
                if (Functions.CompareStrings(finalScoreText, "FIHNLSCORE") >= 40)
                {
                    Functions.DebugMessage("Recognized final score");
                    Vars.gameData.state = State.Finished;
                    Vars.gameData.timer.Stop();
                    Vars.gameData.gameTimer.Stop();
                    Vars.getInfoTimeout.Restart();
                    Vars.gameData.heroesPlayed[Vars.gameData.heroesPlayed.Count - 1].time = (int)Vars.gameData.heroTimer.Elapsed.TotalSeconds;
                    Program.trayMenu.ChangeTray("Visit play menu to upload last game", Resources.Icon_Wait);
                }
            }
        }
        public static void ReadGameScore(Bitmap frame)
        {
            if (Vars.gameData.team1Score == 0 && Vars.gameData.team1Score == 0)
            {
                string scoreTextLeft = Functions.BitmapToText(frame, 800, 560, 95, 135, false, 45, Network.TeamSkillRating);
                string scoreTextRight = Functions.BitmapToText(frame, 1000, 560, 95, 135, false, 45, Network.TeamSkillRating);
                scoreTextLeft = Regex.Match(scoreTextLeft, "[0-9]+").ToString();
                scoreTextRight = Regex.Match(scoreTextRight, "[0-9]+").ToString();

                if (int.TryParse(scoreTextLeft, out int team1) &&
                    int.TryParse(scoreTextRight, out int team2) &&
                    team1 >= 0 && team1 <= 6 && team2 >= 0 && team2 <= 6)
                {
                    Vars.gameData.team1Score = team1;
                    Vars.gameData.team2Score = team2;
                    Vars.loopDelay = 250;
                    Functions.DebugMessage("Recognized team score Team 1:" + scoreTextLeft + " Team 2:" + scoreTextRight);
                    Vars.gameData.state = State.WaitForUpload;
                    Vars.getInfoTimeout.Stop();
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
                        Vars.gameData.players.Clear();
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
                    Vars.gameData.players.Add(new Player(playerName, resultRank[0].ToString()));
                }
                playerNameX += 945;
                playerRankX += 422;
            }
        }
        public static bool IsValidGame()
        {
            if (Vars.gameData.timer.Elapsed.TotalSeconds < 300)
            {
                if (Vars.gameData.state >= State.Recording)
                {
                    Functions.DebugMessage("Invalid game");
                    Program.trayMenu.ChangeTray("Ready to record, enter a competitive game to begin", Resources.Icon_Active);
                }
                return false;
            }
            return true;
        }
    }
}
