using System;
using System.Diagnostics;
using System.Drawing;
using BetterOverwatch.DataObjects;
using BetterOverwatch.Game.Objects;
using Newtonsoft.Json;

namespace BetterOverwatch.Game
{
    class GameData : CompetitiveGame
    {
        public GameData(Ratings currentRatings = null)
        {
            if (currentRatings == null)
            {
                currentRatings = new Ratings();
            }
            this.currentRatings.tank = currentRatings.tank;
            this.currentRatings.damage = currentRatings.damage;
            this.currentRatings.support = currentRatings.support;
            privateToken = AppData.settings.privateToken;
        }
        [JsonIgnore]
        public Ratings currentRatings = new Ratings();
        [JsonIgnore]
        public Ratings startRatings = new Ratings();
        [JsonIgnore]
        public int objectiveTicks = 0;
        [JsonIgnore]
        public Stopwatch timer = new Stopwatch();
        [JsonIgnore]
        public Stopwatch gameTimer = new Stopwatch();
        [JsonIgnore]
        public Stopwatch heroTimer = new Stopwatch();
        [JsonIgnore]
        public Stopwatch tabTimer = new Stopwatch();
        [JsonIgnore]
        public bool tabPressed = false;
        [JsonIgnore]
        public State state = State.Idle;
        [JsonIgnore]
        public Bitmap playerListImage = null;

        public override string ToString()
        {
            battleTag = BattleTag.ReadFromMemory();
            duration = (int)timer.Elapsed.TotalSeconds;

            foreach(HeroPlayed hero in heroesPlayed)
            {
                if (hero.RolePlayed() == "tank")
                {
                    startRating = startRatings.tank;
                    endRating = currentRatings.tank;
                    break;
                }
                else if (hero.RolePlayed() == "damage")
                {
                    startRating = startRatings.damage;
                    endRating = currentRatings.damage;
                    break;
                }
                else if (hero.RolePlayed() == "support")
                {
                    startRating = startRatings.support;
                    endRating = currentRatings.support;
                    break;
                }
            }
            if (AppData.settings.uploadScreenshot && playerListImage != null)
            {
                playerListImageBase64 = Convert.ToBase64String(Functions.ImageToBytes(Functions.ReduceImageSize(playerListImage, 70)));
            }
            if (players.Count != 12) players.Clear();

            return JsonConvert.SerializeObject(this, Formatting.None);
        }
        public bool IsKoth()
        {
            return map.Equals("Busan") || map.Equals("Ilios") || map.Equals("Lijiang Tower") || map.Equals("Nepal") || map.Equals("Oasis");
        }
    }
}