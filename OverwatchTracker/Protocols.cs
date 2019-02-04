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
            string srText = Functions.BitmapToText(frame, 1100, 444, 100, 40, contrastFirst: true, radius: 110, network: 2); // GROUP CHECK
            srText = Regex.Match(srText, "[0-9]+").ToString();

            if (srText.Length < 4)
            {
                srText = srText = Functions.BitmapToText(frame, 1100, 504, 100, 32, contrastFirst: true, radius: 110, network: 2); // SOLO CHECK
                srText = Regex.Match(srText, "[0-9]+").ToString();
            }
            if (srText.Length > 4)
            {
                srText = srText.Substring(srText.Length - 4);
            }

            if (!srText.Equals(String.Empty) && srText.Length <= 4) // CHECK SR
            {
                if (Convert.ToInt32(srText) > 1000 && Convert.ToInt32(srText) < 5000)
                {
                    if (!Vars.srCheck[0].Equals(srText) || !Vars.srCheck[1].Equals(srText))
                    {
                        if (Vars.srCheckIndex > Vars.srCheck.Length - 1)
                        {
                            Vars.srCheckIndex = 0;
                        }
                        Vars.srCheck[Vars.srCheckIndex] = srText;
                        Vars.srCheckIndex++;
                    }
                    if (Vars.srCheck[0].Equals(srText) && Vars.srCheck[1].Equals(srText))
                    {
                        if (!Vars.gameData.currentSkillRating.Equals(srText) || Vars.gameData.gameState >= Vars.STATUS_FINISHED)
                        {
                            Functions.PlaySound();
                            Program.contextMenu.currentGame.MenuItems[1].Text = "Skill rating: " + srText;
                            Functions.DebugMessage("Recognized sr: '" + srText + "'");
                        }
                        Vars.gameTimer.Stop();
                        Vars.srCheck[0] = "";
                        Vars.srCheck[1] = "";
                        Vars.srCheckIndex = 0;
                        Vars.gameData.currentSkillRating = srText;

                        if (Vars.gameData.gameState >= 2)
                        {
                            if (!IsValidGame()) return;
                            Server.UploadGame(Vars.gameData.GetData());
                            ResetGame();
                        }
                        Program.contextMenu.trayIcon.Text = "Ready to record, enter a competitive game to begin";
                        Program.contextMenu.trayIcon.Icon = Properties.Resources.IconActive;
                    }
                }
            }
        }
        public static void CheckStats(Bitmap frame)
        {
            short threshold = 110;
            string elimsText = Functions.BitmapToText(frame, 130, 895, 40, 22, contrastFirst: false, radius: threshold, network: 3);

            if (!elimsText.Equals(String.Empty))
            {
                string damageText = Functions.BitmapToText(frame, 130, 957, 80, 22, contrastFirst: false, radius: threshold, network: 3);

                if (!damageText.Equals(String.Empty))
                {
                    string objKillsText = Functions.BitmapToText(frame, 375, 895, 40, 22, contrastFirst: false, radius: threshold, network: 3);

                    if (!objKillsText.Equals(String.Empty))
                    {
                        string healingText = Functions.BitmapToText(frame, 375, 957, 80, 22, contrastFirst: false, radius: threshold, network: 3);

                        if (!healingText.Equals(String.Empty))
                        {
                            string deathsText = Functions.BitmapToText(frame, 625, 957, 40, 22, contrastFirst: false, radius: threshold, network: 3);

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
                                        Vars.gameData.statsRecorded.Add(
                                            new StatsData(
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
                    if (Vars.gameData.gameState == Vars.STATUS_FINISHED || Vars.gameData.gameState == Vars.STATUS_WAITFORUPLOAD) // a game finished
                    {
                        Server.UploadGame(Vars.gameData.GetData());
                        Vars.gameData = new GameData(Vars.gameData.currentSkillRating);
                        ResetGame();
                    }
                    Vars.getInfoTimeout.Restart();
                    Vars.gameData.gameState = Vars.STATUS_INGAME;
                    Vars.gameData.startsr = Vars.gameData.currentSkillRating;

                    Functions.DebugMessage("Recognized competitive game");
                }
            }
        }
        public static void CheckMap(Bitmap frame)
        {
            if (Vars.gameData.map.Equals(String.Empty))
            {
                string mapText = Functions.BitmapToText(frame, 915, 945, 780, 85);

                if (!mapText.Equals(String.Empty))
                {
                    mapText = Functions.CheckMaps(mapText);

                    if (!mapText.Equals(String.Empty))
                    {
                        Vars.gameData.map = mapText;
                        Program.contextMenu.currentGame.MenuItems[2].Text = "Map: " + mapText;
                        Functions.DebugMessage("Recognized map: '" + mapText + "'");
                        if (mapText.Equals("Ilios") || mapText.Equals("Lijiang Tower") || mapText.Equals("Nepal") || mapText.Equals("Oasis") || mapText.Equals("Busan")) // checks if the map is KOTH
                        {
                            Vars.gameData.iskoth = true;
                        }
                        else
                        {
                            Vars.gameData.iskoth = false;
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
            if (Vars.gameData.team1sr.Equals(String.Empty))
            {
                string team1SR = Functions.BitmapToText(frame, 545, 220, 245, 70, contrastFirst: false, radius: 90, network: 1);
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
                            Vars.gameData.team1sr = team1SR;
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
            if (Vars.gameData.team2sr.Equals(String.Empty))
            {
                string team2SR = Functions.BitmapToText(frame, 1135, 220, 245, 70, contrastFirst: false, radius: 90, network: 1);
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
                            Vars.gameData.team2sr = team2SR;
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
            if (!Vars.gameData.team1sr.Equals(String.Empty) && !Vars.gameData.team1sr.Equals(String.Empty))
            {
                Program.contextMenu.currentGame.MenuItems[3].Text = "Team ratings: " + Vars.gameData.team1sr + " | " + Vars.gameData.team2sr;
            }
            else if (!Vars.gameData.team1sr.Equals(String.Empty))
            {
                Program.contextMenu.currentGame.MenuItems[3].Text = "Team ratings: " + Vars.gameData.team1sr + " | -";
            }
            else if (!Vars.gameData.team2sr.Equals(String.Empty))
            {
                Program.contextMenu.currentGame.MenuItems[3].Text = "Team ratings: - | " + Vars.gameData.team2sr;
            }
        }
        public static void CheckMainMenu(Bitmap frame)
        {
            string menuText = Functions.BitmapToText(frame, 50, 234, 118, 58, contrastFirst: false, radius: 140);

            if (!menuText.Equals(String.Empty))
            {
                if (menuText.Equals("PRAY"))
                {
                    Functions.DebugMessage("Recognized main menu");
                    if (!IsValidGame())
                    {
                        return;
                    }
                    Vars.loopDelay = 250;
                    Program.contextMenu.trayIcon.Text = "Visit play menu to upload last game";
                    Program.contextMenu.trayIcon.Icon = Properties.Resources.IconVisitMenu;
                    Vars.gameData.gameState = Vars.STATUS_FINISHED;
                    Vars.gameTimer.Stop();
                    Vars.heroTimer.Stop();

                    for (int i = 0; i < Vars.gameData.heroesPlayed.Count; i++)
                    {
                        Vars.gameData.heroesTimePlayed[i].Stop();
                    }
                }
            }
        }
        public static bool CheckHeroPlayed(Bitmap frame)
        {
            bool heroDetected = false;
            string heroText = Functions.BitmapToText(frame, 955, 834, 170, 35, contrastFirst: false, radius: 200, network: 4);

            if (!heroText.Equals(String.Empty))
            {
                for (int i = 0; i < Vars.heroNames.Length; i++)
                {
                    if (heroText.Equals("UVR") || heroText.Equals("OVR")) { heroText = "DVA"; }
                    double accuracy = Functions.CompareStrings(heroText, Vars.heroNames[i]);

                    if (accuracy >= 70)
                    {
                        heroDetected = true;
                        if (Vars.gameData.currentHero != i)
                        {
                            bool timerCreated = false;

                            for (int e = 0; e < Vars.gameData.heroesPlayed.Count; e++)
                            {
                                if (Vars.gameData.currentHero != -1)
                                {
                                    if (Vars.gameData.currentHero == Vars.gameData.heroesPlayed[e])
                                    {
                                        Vars.gameData.heroesTimePlayed[e].Stop();
                                        Functions.DebugMessage("Stopped timer for hero " + (e + 1));
                                    }
                                }
                                if (i == Vars.gameData.heroesPlayed[e])
                                {
                                    Vars.gameData.heroesTimePlayed[e].Start();
                                    Functions.DebugMessage("Resumed timer for hero " + (e + 1));
                                    timerCreated = true;
                                }
                            }
                            if (!timerCreated)
                            {
                                Functions.DebugMessage("First time on hero " + (i + 1) + ", creating timer");
                                Vars.gameData.heroesPlayed.Add(i);
                                Vars.gameData.heroesTimePlayed.Add(new Stopwatch());
                                Vars.gameData.heroesTimePlayed[Vars.gameData.heroesTimePlayed.Count - 1].Start();
                            }
                            if (!Vars.heroTimer.IsRunning)
                            {
                                Vars.heroTimer.Restart();
                            }
                            Vars.gameData.currentHero = i;
                            Program.contextMenu.currentGame.MenuItems[4].Text = "Last Hero: " + Vars.heroNamesReal[i];
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
                if (Functions.CompareStrings(finalScoreText, "FINALSCORE") >= 40)
                {
                    Functions.DebugMessage("Recognized final score");
                    if (!IsValidGame())
                    {
                        return;
                    }
                    Program.contextMenu.trayIcon.Text = "Visit play menu to upload last game";
                    Program.contextMenu.trayIcon.Icon = Properties.Resources.IconVisitMenu;
                    Vars.gameData.gameState = Vars.STATUS_FINISHED;
                    Vars.gameTimer.Stop();
                    Vars.heroTimer.Stop();

                    for (int i = 0; i < Vars.gameData.heroesPlayed.Count; i++)
                    {
                        Vars.gameData.heroesTimePlayed[i].Stop();
                    }
                    Thread.Sleep(500);
                }
            }
        }
        public static void CheckGameScore(Bitmap frame)
        {
            if (Vars.gameData.team1score.Equals(String.Empty) && Vars.gameData.team1score.Equals(String.Empty))
            {
                Thread.Sleep(1000); //hackfix, wait 1 second just in case
                string scoreTextLeft = Functions.BitmapToText(frame, 800, 560, 95, 135, contrastFirst: false, radius: 45, network: 1);
                string scoreTextRight = Functions.BitmapToText(frame, 1000, 560, 95, 135, contrastFirst: false, radius: 45, network: 1);
                scoreTextLeft = Regex.Match(scoreTextLeft, "[0-9]+").ToString();
                scoreTextRight = Regex.Match(scoreTextRight, "[0-9]+").ToString();

                if (!scoreTextLeft.Equals(String.Empty) && !scoreTextRight.Equals(String.Empty))
                {
                    int team1 = Convert.ToInt16(scoreTextLeft);
                    int team2 = Convert.ToInt16(scoreTextRight);

                    if (team1 >= 0 && team1 <= 6 && team2 >= 0 && team2 <= 6)
                    {
                        Vars.gameData.team1score = scoreTextLeft;
                        Vars.gameData.team2score = scoreTextRight;
                        Vars.loopDelay = 250;
                        Functions.DebugMessage("Recognized team score Team 1:" + scoreTextLeft + " Team 2:" + scoreTextRight);
                        Program.contextMenu.currentGame.MenuItems[5].Text = "Final score: " + scoreTextLeft + " | " + scoreTextRight;
                        Vars.gameData.gameState = Vars.STATUS_WAITFORUPLOAD;
                    }
                }
            }
        }
        private static bool IsValidGame()
        {
            if (Vars.gameTimer.ElapsedMilliseconds / 1000 < 300)
            {
                if (Vars.gameData.gameState >= Vars.STATUS_RECORDING)
                {
                    Functions.DebugMessage("Invalid game");
                    ResetGame();
                }
                return false;
            }
            return true;
        }
        private static void ResetGame()
        {
            Vars.gameData = new GameData(Vars.gameData.currentSkillRating);
            Program.contextMenu.trayIcon.Text = "Ready to record, enter a competitive game to begin";
            Program.contextMenu.trayIcon.Icon = Properties.Resources.IconActive;
        }
    }
}
