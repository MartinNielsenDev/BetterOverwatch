using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;

namespace BetterOverwatch
{
    class Protocols
    {
        public static void CheckPlayMenu(Bitmap frame)
        {
            string srText = Functions.BitmapToText(frame, 1100, 444, 100, 40, contrastFirst: true, radius: 110, network: Network.Numbers); // GROUP CHECK
            srText = Regex.Match(srText, "[0-9]+").ToString();

            if (srText.Length < 4)
            {
                srText = srText = Functions.BitmapToText(frame, 1100, 504, 100, 32, contrastFirst: true, radius: 110, network: Network.Numbers); // SOLO CHECK
                srText = Regex.Match(srText, "[0-9]+").ToString();
            }
            if (srText.Length > 4)
            {
                srText = srText.Substring(srText.Length - 4);
            }

            if (!srText.Equals(String.Empty) && srText.Length == 4)
            {
                if (Convert.ToInt32(srText) > 1000 && Convert.ToInt32(srText) < 5000)
                {
                    if (!Vars.gameData.currentRating.Equals(srText) || Vars.gameData.state >= State.Finished)
                    {
                        Functions.PlaySound();
                        Program.trayMenu.currentGame.MenuItems[1].Text = "Skill rating: " + srText;
                        Functions.DebugMessage("Recognized sr: '" + srText + "'");
                    }
                    Vars.gameTimer.Stop();
                    Vars.gameData.currentRating = srText;

                    if (Vars.gameData.state == State.Recording ||
                        Vars.gameData.state == State.Finished ||
                        Vars.gameData.state == State.WaitForUpload
                        )
                    {
                        if (!IsValidGame()) return;
                        string game = Vars.gameData.GetData();
                        Vars.lastGameJSON = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<CleanedGame>(game), Formatting.Indented);
                        Server.UploadGame(game);
                        ResetGame();
                    }
                    Program.trayMenu.ChangeTray("Ready to record, enter a competitive game to begin", Properties.Resources.IconActive);
                }
            }
        }
        public static void CheckStats(Bitmap frame)
        {
            short threshold = 110;
            string elimsText = Functions.BitmapToText(frame, 130, 895, 40, 22, contrastFirst: false, radius: threshold, network: Network.Numbers);

            if (!elimsText.Equals(String.Empty))
            {
                string damageText = Functions.BitmapToText(frame, 130, 957, 80, 22, contrastFirst: false, radius: threshold, network: Network.Numbers);

                if (!damageText.Equals(String.Empty))
                {
                    string objKillsText = Functions.BitmapToText(frame, 375, 895, 40, 22, contrastFirst: false, radius: threshold, network: Network.Numbers);

                    if (!objKillsText.Equals(String.Empty))
                    {
                        string healingText = Functions.BitmapToText(frame, 375, 957, 80, 22, contrastFirst: false, radius: threshold, network: Network.Numbers);

                        if (!healingText.Equals(String.Empty))
                        {
                            string deathsText = Functions.BitmapToText(frame, 625, 957, 40, 22, contrastFirst: false, radius: threshold, network: Network.Numbers);

                            if (!deathsText.Equals(String.Empty))
                            {
                                if (Vars.statsCheck[0].Equals(elimsText) &&
                                    Vars.statsCheck[1].Equals(damageText) &&
                                    Vars.statsCheck[2].Equals(objKillsText) &&
                                    Vars.statsCheck[3].Equals(healingText) &&
                                    Vars.statsCheck[4].Equals(deathsText))
                                {
                                    if (Vars.statsTimer.ElapsedMilliseconds > 30000 &&
                                        !(elimsText == "0" && damageText == "0" && objKillsText == "0" && healingText == "0" && deathsText == "0") &&
                                        Vars.gameTimer.ElapsedMilliseconds - Functions.GetTimeDeduction() > 30000 &&
                                        Functions.CheckStats(elimsText, damageText, objKillsText, healingText, deathsText, Vars.gameTimer.ElapsedMilliseconds - Functions.GetTimeDeduction()))
                                    {
                                        Vars.gameData.stats.Add(
                                            new Game.Stats(
                                                elimsText,
                                                damageText,
                                                objKillsText,
                                                healingText,
                                                deathsText,
                                                Vars.gameTimer.ElapsedMilliseconds - Functions.GetTimeDeduction()
                                                ));
                                        Vars.statsTimer.Restart();
                                        Vars.statsCheck[0] = "";
                                        Vars.statsCheck[1] = "";
                                        Vars.statsCheck[2] = "";
                                        Vars.statsCheck[3] = "";
                                        Vars.statsCheck[4] = "";
                                    }
                                }
                                else
                                {
                                    Vars.statsCheck[0] = elimsText;
                                    Vars.statsCheck[1] = damageText;
                                    Vars.statsCheck[2] = objKillsText;
                                    Vars.statsCheck[3] = healingText;
                                    Vars.statsCheck[4] = deathsText;
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void CheckCompetitiveGameEntered(Bitmap frame)
        {
            string compText = Functions.BitmapToText(frame, 1354, 892, 323, 48, contrastFirst: false, radius: 120, network: 0, invertColors: false, red: 255, green: 255, blue: 0);

            if (!compText.Equals(String.Empty))
            {
                double percent = Functions.CompareStrings(compText, "COMPETITIVEPLAY");

                if (percent >= 70)
                {
                    if (Vars.gameData.state == State.Finished || Vars.gameData.state == State.WaitForUpload) // a game finished
                    {
                        string game = Vars.gameData.GetData();
                        Vars.lastGameJSON = JsonConvert.SerializeObject(JsonConvert.DeserializeObject<CleanedGame>(game), Formatting.Indented);
                        Server.UploadGame(game);
                        Vars.gameData = new Game(Vars.gameData.currentRating);
                        ResetGame();
                    }
                    Vars.getInfoTimeout.Restart();
                    Vars.gameData.state = State.Ingame;
                    Vars.gameData.startRating = Vars.gameData.currentRating;

                    Functions.DebugMessage("Recognized competitive game");
                }
            }
        }
        public static void CheckMap(Bitmap frame)
        {
            if (Vars.gameData.mapInfo.mapName.Equals(String.Empty))
            {
                string mapText = Functions.BitmapToText(frame, 915, 945, 780, 85);

                if (!mapText.Equals(String.Empty))
                {
                    mapText = Functions.CheckMaps(mapText);

                    if (!mapText.Equals(String.Empty))
                    {
                        Vars.gameData.mapInfo.mapName = mapText;
                        Program.trayMenu.currentGame.MenuItems[2].Text = "Map: " + mapText;
                        Functions.DebugMessage("Recognized map: '" + mapText + "'");
                        if (mapText.Equals("Ilios") || mapText.Equals("Lijiang Tower") || mapText.Equals("Nepal") || mapText.Equals("Oasis") || mapText.Equals("Busan")) // checks if the map is KOTH
                        {
                            Vars.gameData.mapInfo.isKoth = true;
                        }
                        else
                        {
                            Vars.gameData.mapInfo.isKoth = false;
                        }
                        Vars.roundTimer.Restart();
                        Vars.gameTimer.Restart();
                        Vars.roundsCompleted = 0;
                    }
                }
            }
        }
        public static void CheckTeamsSkillRating(Bitmap frame)
        {
            if (Vars.gameData.team1Rating.Equals(String.Empty))
            {
                string team1SR = Functions.BitmapToText(frame, 545, 220, 245, 70, contrastFirst: false, radius: 90, network: Network.TeamSkillRating);
                team1SR = Regex.Match(team1SR, "[0-9]+").ToString();

                if (!team1SR.Equals(String.Empty) && team1SR.Length >= 4) // TEAM 1 SR
                {
                    team1SR = team1SR.Substring(team1SR.Length - 4);

                    if (Convert.ToInt32(team1SR) > 999 && Convert.ToInt32(team1SR) < 5000)
                    {
                        if (Vars.team1Check[0].Equals(team1SR) && Vars.team1Check[1].Equals(team1SR))
                        {
                            Vars.team1Check[0] = "";
                            Vars.team1Check[1] = "";
                            Vars.gameData.team1Rating = team1SR;
                            Functions.DebugMessage("Recognized team 1 SR: '" + team1SR + "'");
                        }
                        else
                        {
                            Vars.team1CheckIndex++;
                            Vars.team1Check[Vars.team1CheckIndex - 1] = team1SR;
                            if (Vars.team1CheckIndex > Vars.team1Check.Length - 1)
                            {
                                Vars.team1CheckIndex = 0;
                            }
                        }
                    }
                }
            }
            if (Vars.gameData.team2Rating.Equals(String.Empty))
            {
                string team2SR = Functions.BitmapToText(frame, 1135, 220, 245, 70, contrastFirst: false, radius: 90, network: Network.TeamSkillRating);
                team2SR = Regex.Match(team2SR, "[0-9]+").ToString();

                if (!team2SR.Equals(String.Empty) && team2SR.Length >= 4) // TEAM 1 SR
                {
                    team2SR = team2SR.Substring(team2SR.Length - 4);

                    if (Convert.ToInt32(team2SR) > 999 && Convert.ToInt32(team2SR) < 5000)
                    {

                        if (Vars.team2Check[0].Equals(team2SR) && Vars.team2Check[1].Equals(team2SR))
                        {
                            Vars.team2Check[0] = "";
                            Vars.team2Check[1] = "";
                            Vars.gameData.team2Rating = team2SR;
                            Functions.DebugMessage("Recognized team 2 SR: '" + team2SR + "'");
                        }
                        else
                        {
                            Vars.team2CheckIndex++;
                            Vars.team2Check[Vars.team2CheckIndex - 1] = team2SR;
                            if (Vars.team2CheckIndex > Vars.team2Check.Length - 1)
                            {
                                Vars.team2CheckIndex = 0;
                            }
                        }
                    }
                }
            }
            if (!Vars.gameData.team1Rating.Equals(String.Empty) && !Vars.gameData.team1Rating.Equals(String.Empty))
            {
                Program.trayMenu.currentGame.MenuItems[3].Text = "Team ratings: " + Vars.gameData.team1Rating + " | " + Vars.gameData.team2Rating;
            }
            else if (!Vars.gameData.team1Rating.Equals(String.Empty))
            {
                Program.trayMenu.currentGame.MenuItems[3].Text = "Team ratings: " + Vars.gameData.team1Rating + " | -";
            }
            else if (!Vars.gameData.team2Rating.Equals(String.Empty))
            {
                Program.trayMenu.currentGame.MenuItems[3].Text = "Team ratings: - | " + Vars.gameData.team2Rating;
            }
        }
        public static void CheckMainMenu(Bitmap frame)
        {
            string menuText = Functions.BitmapToText(frame, 50, 234, 118, 58, contrastFirst: false, radius: 140);

            if (!menuText.Equals(String.Empty))
            {
                if (menuText.Equals("PLAY"))
                {
                    Functions.DebugMessage("Recognized main menu");
                    if (!IsValidGame()) return;
                    Vars.loopDelay = 250;
                    Program.trayMenu.ChangeTray("Visit play menu to upload last game", Properties.Resources.IconVisitMenu);
                    Vars.gameData.state = State.Finished;
                    Vars.gameTimer.Stop();
                    Vars.heroTimer.Stop();

                    for (int i = 0; i < Vars.gameData.heroPlayed.Count; i++)
                    {
                        Vars.gameData.heroTimePlayed[i].Stop();
                    }
                }
            }
        }
        public static bool CheckHeroPlayed(Bitmap frame)
        {
            bool heroDetected = false;
            string heroText = Functions.BitmapToText(frame, 955, 834, 170, 35, contrastFirst: false, radius: 200, network: Network.HeroNames);

            if (!heroText.Equals(String.Empty))
            {
                for (int h = 0; h < Vars.heroNames.Length; h++)
                {
                    double accuracy = Functions.CompareStrings(heroText, Vars.heroNames[h]);

                    if (accuracy >= 70)
                    {
                        heroDetected = true;
                        if (Vars.gameData.currentHero != h)
                        {
                            bool timerCreated = false;

                            for (int t = 0; t < Vars.gameData.heroPlayed.Count; t++)
                            {
                                if (Vars.gameData.currentHero != -1)
                                {
                                    if (Vars.gameData.currentHero == Vars.gameData.heroPlayed[t])
                                    {
                                        Vars.gameData.heroTimePlayed[t].Stop();
                                    }
                                }
                                if (h == Vars.gameData.heroPlayed[t])
                                {
                                    Vars.gameData.heroTimePlayed[t].Start();
                                    timerCreated = true;
                                }
                            }
                            if (!timerCreated)
                            {
                                Vars.gameData.heroPlayed.Add(h);
                                Vars.gameData.heroTimePlayed.Add(new Stopwatch());
                                Vars.gameData.heroTimePlayed[Vars.gameData.heroTimePlayed.Count - 1].Start();
                            }
                            if (!Vars.heroTimer.IsRunning)
                            {
                                Vars.heroTimer.Restart();
                            }
                            Vars.gameData.currentHero = h;
                            Program.trayMenu.currentGame.MenuItems[4].Text = "Last Hero: " + Vars.heroNamesReal[h];
                            break;
                        }
                    }
                }
            }
            return heroDetected;
        }
        public static void CheckRoundCompleted(Bitmap frame)
        {
            string roundCompletedText = Functions.BitmapToText(frame, 940, 160, 290, 76);

            if (!roundCompletedText.Equals(String.Empty))
            {
                if (Functions.CompareStrings(roundCompletedText, "COMPLETED") >= 70)
                {
                    Vars.roundsCompleted++;
                    Vars.roundTimer.Restart();
                    Functions.DebugMessage($"Recognized round {Vars.roundsCompleted} completed");
                }
            }
        }
        public static void CheckFinalScore(Bitmap frame)
        {
            string finalScoreText = Functions.BitmapToText(frame, 870, 433, 180, 40);

            if (!finalScoreText.Equals(String.Empty))
            {
                if (Functions.CompareStrings(finalScoreText, "FIHNLSCORE") >= 40)
                {
                    Functions.DebugMessage("Recognized final score");
                    if (!IsValidGame())
                    {
                        return;
                    }
                    Program.trayMenu.ChangeTray("Visit play menu to upload last game", Properties.Resources.IconVisitMenu);
                    Vars.gameData.state = State.Finished;
                    Vars.gameTimer.Stop();
                    Vars.heroTimer.Stop();
                    Vars.getInfoTimeout.Restart();

                    for (int i = 0; i < Vars.gameData.heroPlayed.Count; i++)
                    {
                        Vars.gameData.heroTimePlayed[i].Stop();
                    }
                }
            }
        }
        public static void CheckGameScore(Bitmap frame)
        {
            if (Vars.gameData.team1Score.Equals(String.Empty) && Vars.gameData.team1Score.Equals(String.Empty))
            {
                string scoreTextLeft = Functions.BitmapToText(frame, 800, 560, 95, 135, contrastFirst: false, radius: 45, network: Network.TeamSkillRating);
                string scoreTextRight = Functions.BitmapToText(frame, 1000, 560, 95, 135, contrastFirst: false, radius: 45, network: Network.TeamSkillRating);
                scoreTextLeft = Regex.Match(scoreTextLeft, "[0-9]+").ToString();
                scoreTextRight = Regex.Match(scoreTextRight, "[0-9]+").ToString();

                if (!scoreTextLeft.Equals(String.Empty) && !scoreTextRight.Equals(String.Empty))
                {
                    int team1 = Convert.ToInt16(scoreTextLeft);
                    int team2 = Convert.ToInt16(scoreTextRight);

                    if (team1 >= 0 && team1 <= 6 && team2 >= 0 && team2 <= 6)
                    {
                        Vars.gameData.team1Score = scoreTextLeft;
                        Vars.gameData.team2Score = scoreTextRight;
                        Vars.loopDelay = 250;
                        Functions.DebugMessage("Recognized team score Team 1:" + scoreTextLeft + " Team 2:" + scoreTextRight);
                        Program.trayMenu.currentGame.MenuItems[5].Text = "Final score: " + scoreTextLeft + " | " + scoreTextRight;
                        Vars.gameData.state = State.WaitForUpload;
                        Vars.getInfoTimeout.Stop();
                    }
                }
            }
        }
        public static void CheckPlayerNamesAndRank(Bitmap frame)
        {
            int playerNameX = 355, playerRankX = 733;

            for (int teams = 0; teams < 2; teams++)
            {
                for (int players = 0; players < 6; players++)
                {
                    string playerName = Functions.BitmapToText(frame, playerNameX, 325 + (players * 75), 260, 43, contrastFirst: true, radius: 110, network: Network.PlayerNames, invertColors: false, red: 255, green: 255, blue: 255, fillOutside: true, limeToWhite: true);

                    if(playerName.Equals(String.Empty))
                    {
                        Vars.gameData.players.Clear();
                        return;
                    }
                    Bitmap rank = frame.Clone(new Rectangle(playerRankX, 331 + (players * 75), 33, 33), frame.PixelFormat);
                    byte[] backgroundColor = Functions.GetPixelAtPosition(rank, 0, 32);
                    Functions.AdjustColors(rank, 100, backgroundColor[0], backgroundColor[1], backgroundColor[2]); // fill rank with white
                    Functions.AdjustColors(rank, 100, backgroundColor[0], backgroundColor[1], backgroundColor[2], false); // fill backgroundColor with black
                    double[] resultRank = new double[2] { -1, -1 };

                    for (int r = 0; r < Vars.ranks.Length; r++)
                    {
                        double high = Functions.CompareTwoBitmaps(rank, Vars.ranks[r]);

                        if (high > resultRank[1])
                        {
                            resultRank[0] = r;
                            resultRank[1] = high;
                        }
                    }
                    Vars.gameData.players.Add(new Game.Player(playerName, resultRank[0].ToString()));
                }
                playerNameX += 945;
                playerRankX += 422;
            }
        }
        private static bool IsValidGame()
        {
            if (Vars.gameTimer.ElapsedMilliseconds / 1000 < 300)
            {
                if (Vars.gameData.state >= State.Recording)
                {
                    Vars.gameData.GetData();
                    Functions.DebugMessage("Invalid game");
                    ResetGame();
                }
                return false;
            }
            return true;
        }
        private static void ResetGame()
        {
            Vars.gameData = new Game(Vars.gameData.currentRating);
            Program.trayMenu.ChangeTray("Ready to record, enter a competitive game to begin", Properties.Resources.IconActive);
        }
    }
}
