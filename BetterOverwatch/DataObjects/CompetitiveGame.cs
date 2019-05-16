using System.Collections.Generic;
using BetterOverwatch.Game.Objects;

namespace BetterOverwatch.DataObjects
{
    class CompetitiveGame
    {
#pragma warning disable 0169
        public string map = string.Empty;
        public int startRating;
        public int endRating;
        public int team1Rating;
        public int team2Rating;
        public int team1Score;
        public int team2Score;
        public int duration;
        public string battleTag = string.Empty;
        public List<HeroPlayed> heroesPlayed = new List<HeroPlayed>();
        public List<Player> players = new List<Player>();
        public List<Stat> stats = new List<Stat>();
        public string playerListImageBase64 = string.Empty;
        public string privateToken = string.Empty;
    }
}