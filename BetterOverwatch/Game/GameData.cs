using System;
using System.Diagnostics;
using System.Drawing;
using BetterOverwatch.DataObjects;
using Newtonsoft.Json;

namespace BetterOverwatch.Game
{
    class GameData : CompetitiveGame
    {
        public GameData(int rating = 0)
        {
            currentRating = rating;
            privateToken = Vars.settings.privateToken;
        }
        [JsonIgnore]
        public int currentRating;
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
            endRating = currentRating;

            if (Vars.settings.uploadScreenshot && playerListImage != null)
            {
                playerListImageBase64 = Convert.ToBase64String(Functions.ImageToBytes(Functions.ReduceImageSize(playerListImage, 70)));
            }
            if (players.Count >= 12)
            {
                // put yourself into the playerlist for accuracy
                if (!battleTag.Equals("PLAYER-0000"))
                {
                    string[] btagSplit = battleTag.Split('-');
                    if (btagSplit.Length > 1)
                    {
                        players[0].name = btagSplit[0].ToUpper();
                    }
                }
            }
            else
            {
                players.Clear();
            }

            return JsonConvert.SerializeObject(this, Formatting.None);
        }
        public bool IsKoth()
        {
            return map.Equals("Busan") || map.Equals("Ilios") || map.Equals("Lijiang Tower") || map.Equals("Nepal") || map.Equals("Oasis");
        }
    }
}