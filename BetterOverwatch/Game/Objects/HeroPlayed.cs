namespace BetterOverwatch.Game.Objects
{
    public class HeroPlayed
    {
        public string name { get; }
        public int startTime { get; set; }
        public int time { get; set; }

        public HeroPlayed(string name, int startTime)
        {
            this.name = name;
            this.startTime = startTime;
        }
    }
}
