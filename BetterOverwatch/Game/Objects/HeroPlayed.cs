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
        public string RolePlayed()
        {
            foreach(Hero hero in Constants.HERO_LIST)
            {
                if(hero.name == name)
                {
                    return hero.role;
                }
            }
            return string.Empty;
        }
    }
}
