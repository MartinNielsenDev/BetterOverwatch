namespace BetterOverwatch.Game.Objects
{
    public class Player
    {
        public string name { get; set; }
        public string rank { get; set; }

        public Player(string name, string rank)
        {
            this.name = name;
            this.rank = rank;
        }
    }
}
