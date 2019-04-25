using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    internal class Game
    {
        public Game(string currentRating = "")
        {
            this.currentRating = currentRating;
        }
        [JsonIgnore]
        public State state = State.Idle;
        [JsonIgnore]
        public int currentHero = -1;
        [JsonIgnore]
        public string currentRating = string.Empty;
        [JsonIgnore]
        public List<Stopwatch> heroTimePlayed = new List<Stopwatch>();
        [JsonIgnore]
        public List<int> heroPlayed = new List<int>();
        [JsonIgnore]
        public Bitmap debugImage = null;
        [JsonIgnore]
        public Bitmap playerListImage = null;

        [JsonProperty("mapInfo")]
        public MapInfo mapInfo = new MapInfo();
        [JsonProperty("startRating")]
        public string startRating = "";
        [JsonProperty("endRating")]
        public string endRating = "";
        [JsonProperty("team1Rating")]
        public string team1Rating = "";
        [JsonProperty("team2Rating")]
        public string team2Rating = "";
        [JsonProperty("team1Score")]
        public string team1Score = "";
        [JsonProperty("team2Score")]
        public string team2Score = "";
        [JsonProperty("duration")]
        public string duration = "";
        [JsonProperty("debugImageBase64")]
        public string debugImageBase64 = "";
        [JsonProperty("playerListImageBase64")]
        public string playerListImageBase64 = "";
        [JsonProperty("players")]
        public List<Player> players = new List<Player>();
        [JsonProperty("statsRecorded")]
        public List<Stats> stats = new List<Stats>();
        [JsonProperty("heroes")]
        private List<HeroPlayed> heroes = new List<HeroPlayed>();
        [JsonProperty("battleTag")]
        public string battleTag = Functions.FetchBattleTag();
        [JsonProperty("privateToken")]
        private readonly string privateToken = Vars.settings.privateToken;

        public string GetData()
        {
            if (heroPlayed.Count > 0)
            {
                for (int i = 0; i < (heroPlayed.Count > 3 ? 3 : heroPlayed.Count); i++)
                {
                    long mostPlayed = 0;
                    int mostPlayedIndex = 0;

                    for (int h = 0; h < heroPlayed.Count; h++)
                    {
                        if (heroPlayed[h] > -1)
                        {
                            if (heroTimePlayed[h].ElapsedMilliseconds / 1000 > mostPlayed)
                            {
                                mostPlayed = heroTimePlayed[h].ElapsedMilliseconds / 1000;
                                mostPlayedIndex = h;
                            }
                        }
                    }
                    if (heroTimePlayed[mostPlayedIndex].ElapsedMilliseconds > 60000)
                    {
                        heroes.Add(
                            new HeroPlayed(
                                heroPlayed[mostPlayedIndex].ToString(),
                                Math.Round(Convert.ToDouble(heroTimePlayed[mostPlayedIndex].ElapsedMilliseconds / 1000) / Convert.ToDouble(Vars.heroTimer.ElapsedMilliseconds / 1000) * 100, 0).ToString()
                                ));
                    }
                    heroPlayed[mostPlayedIndex] = -1;
                }
            }
            duration = Math.Floor(Convert.ToDouble(Vars.gameTimer.ElapsedMilliseconds / 1000)).ToString();
            endRating = currentRating;

            if (Vars.settings.uploadScreenshot && playerListImage != null)
            {
                playerListImageBase64 = Convert.ToBase64String(Functions.ImageToBytes(Functions.ReduceImageSize(playerListImage, 70)));
            }
            if(Vars.settings.uploadScreenshot && debugImage != null)
            {
                debugImageBase64 = Convert.ToBase64String(Functions.ImageToBytes(debugImage));
            }
            if(players.Count >= 12)
            {
                if(!battleTag.Equals("PLAYER-0000"))
                {
                    string[] btagSplit = battleTag.Split('-');
                    if (btagSplit.Length > 1)
                    {
                        players[0].playerName = btagSplit[0];
                    }
                }
            }
            else
            {
                players.Clear();
            }
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public class Stats
        {
            [JsonProperty("playerElims")]
            public string playerElims { get; set; }
            [JsonProperty("playerDamage")]
            public string playerDamage { get; set; }
            [JsonProperty("playerObjKills")]
            public string playerObjKills { get; set; }
            [JsonProperty("playerHealing")]
            public string playerHealing { get; set; }
            [JsonProperty("playerDeaths")]
            public string playerDeaths { get; set; }
            [JsonProperty("time")]
            public int time { get; set; }

            public Stats(string elims, string damage, string objective, string healing, string deaths, double t)
            {
                playerElims = elims;
                playerDamage = damage;
                playerObjKills = objective;
                playerHealing = healing;
                playerDeaths = deaths;
                time = Convert.ToInt32(Math.Floor(t / 1000));
            }
        }
        public class Player
        {
            [JsonProperty("playerName")]
            public string playerName { get; set; }
            [JsonProperty("playerRank")]
            public string playerRank { get; set; }

            public Player(string playerName, string playerRank)
            {
                this.playerName = playerName;
                this.playerRank = playerRank;
            }
        }
        public class HeroPlayed
        {
            [JsonProperty("heroIndex")]
            private string heroIndex { get; }
            [JsonProperty("heroPercentPlayed")]
            private string heroPercentPlayed { get; }

            public HeroPlayed(string heroIndex, string heroPercentPlayed)
            {
                this.heroIndex = heroIndex;
                this.heroPercentPlayed = heroPercentPlayed;
            }
        }
        public class MapInfo
        {
            [JsonProperty("mapName")]
            public string mapName = "";
            [JsonProperty("isKoth")]
            public bool isKoth;
        }
    }
}