namespace BetterOverwatch.Game.Objects
{
    public class Stat
    {
        public string heroPlayed = AppData.gameData.heroesPlayed[AppData.gameData.heroesPlayed.Count - 1].name;
        public int time { get; }
        public int eliminations { get; }
        public int objectiveKills { get; }
        public int deaths { get; }
        public int damage { get; }
        public int healing { get; }
        public int[] heroStats { get; }

        public Stat(int time, int eliminations, int damage, int objectiveKills, int healing, int deaths, int[] heroStats)
        {
            this.time = time;

            // general stats
            this.eliminations = eliminations;
            this.damage = damage;
            this.objectiveKills = objectiveKills;
            this.healing = healing;
            this.deaths = deaths;

            // hero specific stats
            this.heroStats = heroStats;
        }
    }
}