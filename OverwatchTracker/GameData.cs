using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    class StatsData
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

        public StatsData(string elims, string damage, string objective, string healing, string deaths, double t)
        {
            playerElims = elims;
            playerDamage = damage;
            playerObjKills = objective;
            playerHealing = healing;
            playerDeaths = deaths;
            time = Convert.ToInt32(Math.Floor(t / 1000));
        }
        public string GetData()
        {
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    class GameData
    {
        public GameData(string currentRating = "")
        {
            // need to save previous current skill rating since the GameData object is disposed and recreated
            this.currentRating = currentRating;
        }
        [JsonIgnore]
        public int gameState = Vars.STATUS_IDLE;
        [JsonIgnore]
        public int currentHero = -1;
        [JsonIgnore]
        public string currentRating = String.Empty;
        [JsonIgnore]
        public List<Stopwatch> heroTimePlayed = new List<Stopwatch>();
        [JsonIgnore]
        public List<int> heroPlayed = new List<int>();
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
        [JsonProperty("playerListImageBase64")]
        public string playerListImageBase64 = "";
        [JsonProperty("heroes")]
        private List<Hero> heroes = new List<Hero>();
        [JsonProperty("statsRecorded")]
        public List<StatsData> statsRecorded = new List<StatsData>();
        [JsonProperty("battleTag")]
        public string battleTag = Functions.FetchBattleTag();
        [JsonProperty("privateToken")]
        private string privateToken = Vars.settings.privateToken;

        public string GetData()
        {
            if (this.heroPlayed.Count > 0)
            {
                for (int i = 0; i < (this.heroPlayed.Count > 3 ? 3 : this.heroPlayed.Count); i++)
                {
                    long mostPlayed = 0;
                    int mostPlayedIndex = 0;

                    for (int h = 0; h < this.heroPlayed.Count; h++)
                    {
                        if (this.heroPlayed[h] > -1)
                        {
                            if (this.heroTimePlayed[h].ElapsedMilliseconds / 1000 > mostPlayed)
                            {
                                mostPlayed = this.heroTimePlayed[h].ElapsedMilliseconds / 1000;
                                mostPlayedIndex = h;
                            }
                        }
                    }
                    if (this.heroTimePlayed[mostPlayedIndex].ElapsedMilliseconds > 6000)
                    {
                        this.heroes.Add(
                            new Hero(
                                this.heroPlayed[mostPlayedIndex].ToString(),
                                Math.Round(Convert.ToDouble(this.heroTimePlayed[mostPlayedIndex].ElapsedMilliseconds / 1000) / Convert.ToDouble(Vars.heroTimer.ElapsedMilliseconds / 1000) * 100, 0).ToString()
                                ));
                    }
                    this.heroPlayed[mostPlayedIndex] = -1;
                }
            }
            this.duration = Math.Floor(Convert.ToDouble(Vars.gameTimer.ElapsedMilliseconds / 1000)).ToString();
            this.endRating = currentRating;

            if (Vars.settings.uploadScreenshot && this.playerListImage != null)
            {
                this.playerListImageBase64 = Convert.ToBase64String(Functions.ImageToBytes(Functions.ReduceImageSize(this.playerListImage, 70)));
            }
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    class Hero
    {
        [JsonProperty("heroIndex")]
        private string heroIndex { get; }
        [JsonProperty("heroPercentPlayed")]
        private string heroPercentPlayed { get;}

        public Hero(string heroIndex, string heroPercentPlayed)
        {
            this.heroIndex = heroIndex;
            this.heroPercentPlayed = heroPercentPlayed;
        }
    }
    class MapInfo
    {
        [JsonProperty("mapName")]
        public string mapName = "";
        [JsonProperty("isKoth")]
        public bool isKoth = false;
    }
}