using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace OverwatchTracker
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

        public StatsData(string elims = "0", string damage = "0", string objective = "0", string healing = "0", string deaths = "0", double t = 0)
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
        [JsonIgnore]
        public int gameState = Vars.STATUS_IDLE;
        [JsonIgnore]
        public int currentHero = -1;
        [JsonIgnore]
        public string currentSkillRating = String.Empty;
        [JsonIgnore]
        public List<Stopwatch> heroesTimePlayed = new List<Stopwatch>();
        [JsonIgnore]
        public List<int> heroesPlayed = new List<int>();
        [JsonIgnore]
        public Bitmap playerlistimage = null;
        [JsonProperty("battletag")]
        public string battletag { get; set; } = Functions.getBattletag();
        [JsonProperty("heroes")]
        public string heroes { get; set; } = "";
        [JsonProperty("startsr")]
        public string startsr { get; set; } = "";
        [JsonProperty("endsr")]
        public string endsr { get; set; } = "";
        [JsonProperty("map")]
        public string map { get; set; } = "";
        [JsonProperty("iskoth")]
        public bool iskoth { get; set; } = false;
        [JsonProperty("team1sr")]
        public string team1sr { get; set; } = "";
        [JsonProperty("team2sr")]
        public string team2sr { get; set; } = "";
        [JsonProperty("team1score")]
        public string team1score { get; set; } = "";
        [JsonProperty("team2score")]
        public string team2score { get; set; } = "";
        [JsonProperty("duration")]
        public string duration { get; set; } = "";
        [JsonProperty("playerlistimagebase64")]
        public string playerlistimagebase64 { get; set; } = "";
        [JsonProperty("privateToken")]
        public string privateToken { get; set; } = Vars.settings.privateToken;
        [JsonProperty("statsRecorded")]
        public List<StatsData> statsRecorded = new List<StatsData>();

        public string GetData()
        {
            string heroesPlayed = String.Empty;

            if (Vars.gameData.heroesPlayed.Count > 0)
            {
                for (int i = 0; i < Vars.gameData.heroesPlayed.Count; i++)
                {
                    if (i >= 3)
                    {
                        break;
                    }
                    long biggestValue = 0;
                    int biggestValueIndex = 0;

                    for (int e = 0; e < Vars.gameData.heroesPlayed.Count; e++)
                    {
                        if (Vars.gameData.heroesPlayed[e] > -1)
                        {
                            if (Vars.gameData.heroesTimePlayed[e].ElapsedMilliseconds / 1000 > biggestValue)
                            {
                                biggestValue = Vars.gameData.heroesTimePlayed[e].ElapsedMilliseconds / 1000;
                                biggestValueIndex = e;
                            }
                        }
                    }
                    if (Vars.gameData.heroesTimePlayed[biggestValueIndex].ElapsedMilliseconds > 10000)
                    {
                        heroesPlayed += Vars.gameData.heroesPlayed[biggestValueIndex] + " " + Math.Round(Convert.ToDouble(Vars.gameData.heroesTimePlayed[biggestValueIndex].ElapsedMilliseconds / 1000) / Convert.ToDouble(Vars.heroTimer.ElapsedMilliseconds / 1000) * 100, 1).ToString().Replace(",", ".") + ",";
                    }
                    Vars.gameData.heroesPlayed[biggestValueIndex] = -1;
                }
                Vars.gameData.heroes = heroesPlayed;
            }
            Vars.gameData.duration = Math.Floor(Convert.ToDouble(Vars.gameTimer.ElapsedMilliseconds / 1000)).ToString();
            Vars.gameData.endsr = currentSkillRating;
            // convert bitmap to base64 so server can decode it
            playerlistimagebase64 = Convert.ToBase64String(Functions.imageToBytes(Functions.reduceImageSize(Vars.gameData.playerlistimage, 70)));
            Debug.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                
            }));
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}