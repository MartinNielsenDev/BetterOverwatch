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
        internal static bool IsOnCompetitiveScreen(Bitmap frame)
        {
            return Functions.CompareStrings(BitmapFunctions.ProcessFrame(frame, Rectangles.CompetitiveScreen, false, 110, NetworkEnum.Maps, false), "COMPETITIVEPLAY") >= 80;
        }
        internal static void ReadRoleRatings(Bitmap frame)
        {
            Rectangle tankRect = Rectangles.TankCheck;
            Rectangle damageRect = Rectangles.DamageCheck;
            Rectangle supportRect = Rectangles.SupportCheck;
            Rectangle tankRatingRect = Rectangles.TankRating;
            Rectangle damageRatingRect = Rectangles.DamageRating;
            Rectangle supportRatingRect = Rectangles.SupportRating;
            bool tankCheck = BitmapFunctions.BitmapIsCertainColor(BitmapFunctions.CropImage(frame, tankRect), 255, 255, 255);
            bool damageCheck = BitmapFunctions.BitmapIsCertainColor(BitmapFunctions.CropImage(frame, damageRect), 255, 255, 255);
            bool supportCheck = BitmapFunctions.BitmapIsCertainColor(BitmapFunctions.CropImage(frame, supportRect), 255, 255, 255);
            int tankRating = 0;
            int damageRating = 0;
            int supportRating = 0;
            string debugString = "";

            if (!tankCheck && !damageCheck && !supportCheck)
            {
                tankRect.X -= 226;
                damageRect.X -= 226;
                supportRect.X -= 226;
                tankRatingRect.X -= 226;
                damageRatingRect.X -= 226;
                supportRatingRect.X -= 226;

                tankCheck = BitmapFunctions.BitmapIsCertainColor(BitmapFunctions.CropImage(frame, tankRect), 255, 255, 255);
                damageCheck = BitmapFunctions.BitmapIsCertainColor(BitmapFunctions.CropImage(frame, damageRect), 255, 255, 255);
                supportCheck = BitmapFunctions.BitmapIsCertainColor(BitmapFunctions.CropImage(frame, supportRect), 255, 255, 255);
            }
            if (tankCheck)
            {
                string tankRatingText = BitmapFunctions.ProcessFrame(frame, tankRatingRect, true, 110, NetworkEnum.Ratings, true);

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
                string damageRatingText = BitmapFunctions.ProcessFrame(frame, damageRatingRect, true, 110, NetworkEnum.Ratings, true);

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
                string supportRatingText = BitmapFunctions.ProcessFrame(frame, supportRatingRect, true, 110, NetworkEnum.Ratings, true);

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
                AppData.gameData.state >= State.Record))
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

                if (AppData.gameData.state >= State.Record)
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
        internal static int[] ReadHeroStats(Bitmap frame, int[] statSettings)
        {
            int[] heroStats = { 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < statSettings.Length; i++)
            {
                if (statSettings[i] > 0)
                {
                    string heroStatText = BitmapFunctions.ProcessFrame(frame, new Rectangle(Constants.HERO_STAT_POSITIONS[i][0], Constants.HERO_STAT_POSITIONS[i][1], statSettings[i] == 1 ? 40 : 80, 21), false, 110, NetworkEnum.Stats);

                    if (int.TryParse(heroStatText, out int heroStat))
                    {
                        heroStats[i] = heroStat;
                    }
                }
            }

            return heroStats;
        }
        internal static void ReadStats(Bitmap frame)
        {
            if (AppData.statsTimer.Elapsed.TotalSeconds < 20) return;
            string eliminationsText = BitmapFunctions.ProcessFrame(frame, Rectangles.EliminationsStat, false, 110, NetworkEnum.Stats);
            if (eliminationsText.Equals(string.Empty)) return;
            string damageText = BitmapFunctions.ProcessFrame(frame, Rectangles.DamageStat, false, 110, NetworkEnum.Stats);
            if (damageText.Equals(string.Empty)) return;
            string objectiveKillsText = BitmapFunctions.ProcessFrame(frame, Rectangles.ObjectiveKillsStat, false, 110, NetworkEnum.Stats);
            if (objectiveKillsText.Equals(string.Empty)) return;
            string healingText = BitmapFunctions.ProcessFrame(frame, Rectangles.HealingStat, false, 110, NetworkEnum.Stats);
            if (healingText.Equals(string.Empty)) return;
            string deathsText = BitmapFunctions.ProcessFrame(frame, Rectangles.DeathsStat, false, 110, NetworkEnum.Stats);
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

                for (int i = 0; i < Constants.HERO_LIST.Length; i++)
                {
                    if (Constants.HERO_LIST[i].name.Equals(AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].name))
                    {
                        heroStats = ReadHeroStats(frame, Constants.HERO_LIST[i].statSettings);
                        break;
                    }
                }
                AppData.gameData.stats.Add(new Stat((int)AppData.gameData.gameTimer.Elapsed.TotalSeconds, eliminations, damage, objectiveKills, healing, deaths, heroStats));
                AppData.statsTimer.Restart();
                Functions.DebugMessage($"Hero stats recorded after {(int)Math.Floor(AppData.gameData.gameTimer.Elapsed.TotalSeconds)} seconds");
            }
        }
        internal static void ReadCompetitiveGameEntered(Bitmap frame)
        {
            string compText = BitmapFunctions.ProcessFrame(frame, Rectangles.CompetitiveEntered, false, 120, 0, false, 255, 255, 0);

            if (!compText.Equals(string.Empty))
            {
                double percent = Functions.CompareStrings(compText, "COMPETITIVEPLAY");

                if (percent >= 70)
                {
                    if (AppData.gameData.state == State.Finished || AppData.gameData.state == State.Upload) // a game finished
                    {
                        Server.CheckGameUpload();
                    }
                    AppData.gameData = new GameData(AppData.gameData.currentRatings);
                    AppData.infoTimer.Restart();
                    AppData.gameData.state = State.Ingame;
                    AppData.gameData.startRatings.tank = AppData.gameData.currentRatings.tank;
                    AppData.gameData.startRatings.damage = AppData.gameData.currentRatings.damage;
                    AppData.gameData.startRatings.support = AppData.gameData.currentRatings.support;

                    Functions.DebugMessage("Recognized competitive game");
                }
            }
        }
        internal static void ReadMap(Bitmap frame)
        {
            if (AppData.gameData.map.Equals(string.Empty))
            {
                string mapText = BitmapFunctions.ProcessFrame(frame, Rectangles.Map);

                if (!mapText.Equals(string.Empty))
                {
                    mapText = Functions.CheckMaps(mapText);

                    if (!mapText.Equals(string.Empty))
                    {
                        AppData.gameData.map = mapText;
                        AppData.infoTimer.Restart();
                        Functions.DebugMessage($"Recognized map: '{mapText}'");
                    }
                }
            }
        }
        internal static void ReadTeamsSkillRating(Bitmap frame)
        {
            if (AppData.gameData.team1Rating == 0)
            {
                string team1Rating = BitmapFunctions.ProcessFrame(frame, Rectangles.Team1Rating, false, 90, NetworkEnum.Ratings);
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
                string team2Rating = BitmapFunctions.ProcessFrame(frame, Rectangles.Team2Rating, false, 90, NetworkEnum.Ratings);
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
        internal static void ReadMainMenu(Bitmap frame)
        {
            string menuText = BitmapFunctions.ProcessFrame(frame, Rectangles.MainMenu, false, 140);

            if (!menuText.Equals(string.Empty))
            {
                if (menuText.Equals("PLAY"))
                {
                    Functions.DebugMessage("Recognized main menu");
                    AppData.loopDelay = 500;
                    AppData.gameData.state = State.Finished;
                    AppData.gameData.timer.Stop();
                    AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time = (int)AppData.gameData.gameTimer.Elapsed.TotalSeconds;
                    ScreenCaptureHandler.trayMenu.ChangeTray("Visit play menu to upload last game", Resources.Icon_Wait);
                }
            }
        }
        internal static bool ReadHeroPlayed(Bitmap frame)
        {
            string heroText = BitmapFunctions.ProcessFrame(frame, Rectangles.Hero, false, 200, NetworkEnum.Heroes);

            if (!heroText.Equals(string.Empty))
            {
                for (int h = 0; h < Constants.HERO_COMPARER_LIST.Length; h++)
                {
                    double accuracy = Functions.CompareStrings(heroText, Constants.HERO_COMPARER_LIST[h]);

                    if (accuracy >= 70)
                    {
                        if (AppData.gameData.heroesPlayed.Count > 0 &&
                            AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].name == Constants.HERO_LIST[h].name)
                        {
                            return true; // if playing the same hero, don't continue
                        }
                        if (AppData.gameData.heroesPlayed.Count > 0)
                        {
                            if ((AppData.gameData.state == State.RoundComplete || AppData.gameData.state == State.RoundStart) &&
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
                        AppData.gameData.heroesPlayed.Add(new HeroPlayed(Constants.HERO_LIST[h].name, (int)AppData.gameData.gameTimer.Elapsed.TotalSeconds));
                        Functions.DebugMessage($"Now playing hero {Constants.HERO_LIST[h].name} at {AppData.gameData.gameTimer.Elapsed.TotalSeconds} seconds");
                        return true;
                    }
                }
            }
            return false;
        }
        internal static void ReadRoundCompleted(Bitmap frame)
        {
            string roundCompletedText = BitmapFunctions.ProcessFrame(frame, Rectangles.RoundCompleted);

            if (roundCompletedText != string.Empty && Functions.CompareStrings(roundCompletedText, "COMPLETE") >= 70)
            {
                if (AppData.gameData.heroesPlayed.Count > 0)
                {
                    AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time = (int)AppData.gameData.heroTimer.Elapsed.TotalSeconds;
                }
                AppData.gameData.state = State.RoundComplete;
                AppData.gameData.heroTimer.Stop();
                AppData.gameData.gameTimer.Stop();
                AppData.infoTimer.Restart();
                Functions.DebugMessage($"Recognized round completed");
            }
        }
        internal static bool ReadRoundStarted(Bitmap frame)
        {
            // hackfix: not actually reading the text, just checking the length
            string roundStartedText = BitmapFunctions.ProcessFrame(frame, Rectangles.RoundStarted, true, 110, NetworkEnum.Maps);

            if (roundStartedText.Length >= 10) AppData.gameData.objectiveTicks++;
            else if (roundStartedText.Length < 4) AppData.gameData.objectiveTicks = 0;

            return AppData.gameData.objectiveTicks >= 4;
        }
        internal static void ReadFinalScore(Bitmap frame)
        {
            string finalScoreText = BitmapFunctions.ProcessFrame(frame, Rectangles.FinalScore, false, 110, NetworkEnum.Heroes);

            if (!finalScoreText.Equals(string.Empty))
            {
                Console.WriteLine(finalScoreText);
                if (Functions.CompareStrings(finalScoreText, "TINALSCORE") >= 50)
                {
                    Functions.DebugMessage("Recognized final score");
                    AppData.gameData.state = State.Finished;
                    AppData.gameData.timer.Stop();
                    AppData.gameData.gameTimer.Stop();
                    AppData.infoTimer.Restart();
                    if (AppData.gameData.heroesPlayed.Count > 0)
                    {
                        AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].time = (int)AppData.gameData.heroTimer.Elapsed.TotalSeconds;
                    }
                    ScreenCaptureHandler.trayMenu.ChangeTray("Visit play menu to upload last game", Resources.Icon_Wait);
                }
            }
        }
        internal static void ReadGameScore(Bitmap frame)
        {
            if (AppData.gameData.team1Score == 0 && AppData.gameData.team1Score == 0 || ScreenCaptureHandler.debug)
            {
                string scoreTextLeft = BitmapFunctions.ProcessFrame(frame, Rectangles.Team1Score, false, 45, NetworkEnum.Ratings);
                string scoreTextRight = BitmapFunctions.ProcessFrame(frame, Rectangles.Team2Score, false, 45, NetworkEnum.Ratings);
                scoreTextLeft = Regex.Match(scoreTextLeft, "[0-9]+").ToString();
                scoreTextRight = Regex.Match(scoreTextRight, "[0-9]+").ToString();

                Console.WriteLine($"Left: {scoreTextLeft} Right: {scoreTextRight}");
                if (int.TryParse(scoreTextLeft, out int team1) &&
                    int.TryParse(scoreTextRight, out int team2) &&
                    team1 >= 0 && team1 <= 6 && team2 >= 0 && team2 <= 6)
                {
                    AppData.gameData.team1Score = team1;
                    AppData.gameData.team2Score = team2;
                    AppData.loopDelay = 500;
                    Functions.DebugMessage("Recognized team score Team 1:" + scoreTextLeft + " Team 2:" + scoreTextRight);
                    AppData.gameData.state = State.Upload;
                    AppData.infoTimer.Stop();
                }
            }
        }
        internal static void ReadPlayerNamesAndRank(Bitmap frame)
        {
            int playerNameX = 355, playerRankX = 733;

            for (int teams = 0; teams < 2; teams++)
            {
                for (int players = 0; players < 6; players++)
                {
                    string playerName = BitmapFunctions.ProcessFrame(frame, new Rectangle(playerNameX, 325 + (players * 75), 260, 43), true, 110, NetworkEnum.Players, false, 255, 255, 255, true, true);

                    if (playerName.Equals(string.Empty))
                    {
                        AppData.gameData.players.Clear();
                        return;
                    }
                    Bitmap rank = frame.Clone(new Rectangle(playerRankX, 331 + (players * 75), 33, 33), frame.PixelFormat);
                    byte[] backgroundColor = BitmapFunctions.GetPixelAtPosition(rank, 0, 32);
                    BitmapFunctions.AdjustColors(rank, 100, backgroundColor[0], backgroundColor[1], backgroundColor[2]); // fill rank with white
                    BitmapFunctions.AdjustColors(rank, 100, backgroundColor[0], backgroundColor[1], backgroundColor[2], false); // fill backgroundColor with black
                    double[] resultRank = new double[2] { -1, -1 };

                    for (int r = 0; r < Constants.RANK_LIST.Length; r++)
                    {
                        double high = Functions.CompareTwoBitmaps(rank, Constants.RANK_LIST[r]);

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
            //Functions.DebugMessage("Captured player list");
        }
        internal static bool IsValidGame()
        {
            if (AppData.gameData.timer.Elapsed.TotalSeconds < 300 && !ScreenCaptureHandler.debug)
            {
                if (AppData.gameData.state >= State.Record)
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
