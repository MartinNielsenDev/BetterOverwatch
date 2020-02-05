using BetterOverwatch.Properties;
using System.Drawing;

namespace BetterOverwatch
{
    class Constants
    {
        internal static string[] MAP_LIST = { "Hanamura", "Horizon Lunar Colony", "Temple of Anubis", "Volskaya Industries", "Blizzard World", "Eichenwalde", "Hollywood", "King's Row", "Numbani", "Ilios", "Lijiang Tower", "Nepal", "Oasis", "Dorado", "Junkertown", "Route 66", "Watchpoint Gibraltar", "Rialto", "Busan", "Paris", "Havana" };
        internal static string[] MAP_LIST_BLACKLIST = { "Ilios Lighthouse", "Ilios Ruins", "Ilios Well", "Lijang Control Center", "Lijang Garden", "Lijang Night Market", "Nepal Sanctum", "Nepal Shrine", "Nepal Village", "Oasis City Center", "Oasis Gardens", "Oasis University"};
        internal static string[] HERO_COMPARER_LIST = { "DOOMFIST", "GENJI", "MCCREE", "PHARAH", "REAPER", "SOLDIERIO", "SOMBRA", "TRACER", "BASTION", "HANZO", "JUNKRAT", "MEI", "TORBJORN", "WIDOWMAKER", "DVA", "ORISA", "REINHARDT", "ROADHOG", "WINSTON", "ZARYA", "ANA", "BRIGITTE", "LUCIO", "MERCY", "MOIRA", "SYMMETRA", "ZENYATTA", "WRECKINGBAL", "ASHE", "BAPTISTE", "SIGMA" };
        internal static Hero[] HERO_LIST =
        {
            new Hero("Doomfist", new int[]
            {
                0,
                2,
                0,
                1,
                1,
                2
            }, "damage"),
            new Hero("Genji", new int[]
            {
                0,
                2,
                0,
                1,
                1,
                0
            }, "damage"),
            new Hero("McCree", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                1
            }, "damage"),
            new Hero("Pharah", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }, "damage"),
            new Hero("Reaper", new int[]
            {
                0,
                1,
                0,
                2,
                1,
                0
            }, "damage"),
            new Hero("Soldier: 76", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }, "damage"),
            new Hero("Sombra", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }, "damage"),
            new Hero("Tracer", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }, "damage"),
            new Hero("Bastion", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                2
            }, "damage"),
            new Hero("Hanzo", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                1
            }, "damage"),
            new Hero("Junkrat", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }, "damage"),
            new Hero("Mei", new int[]
            {
                2,
                1,
                0,
                1,
                1,
                0
            }, "damage"),
            new Hero("Torbjörn", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                2
            }, "damage"),
            new Hero("Widowmaker", new int[]
            {
                1,
                0,
                0,
                1,
                1,
                0
            }, "damage"),
            new Hero("D.va", new int[]
            {
                0,
                1,
                0,
                1,
                2,
                0
            }, "tank"),
            new Hero("Orisa", new int[]
            {
                0,
                1,
                0,
                2,
                2,
                0
            }, "tank"),
            new Hero("Reinhardt", new int[]
            {
                2,
                1,
                0,
                1,
                1,
                0
            }, "tank"),
            new Hero("Roadhog", new int[]
            {
                0,
                0,
                0,
                2,
                1,
                1
            }, "tank"),
            new Hero("Winston", new int[]
            {
                2,
                1,
                0,
                1,
                1,
                0
            }, "tank"),
            new Hero("Zarya", new int[]
            {
                2,
                0,
                0,
                1,
                1,
                0
            }, "tank"),
            new Hero("Ana", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }, "support"),
            new Hero("Brigitte", new int[]
            {
                1,
                1,
                1,
                0,
                2,
                0
            }, "support"),
            new Hero("Lúcio", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }, "support"),
            new Hero("Mercy", new int[]
            {
                1,
                1,
                1,
                2,
                1,
                0
            }, "support"),
            new Hero("Moira", new int[]
            {
                0,
                1,
                0,
                2,
                1,
                2
            }, "support"),
            new Hero("Symmetra", new int[]
            {
                1,
                1,
                0,
                0,
                2,
                0
            }, "damage"),
            new Hero("Zenyatta", new int[]
            {
                0,
                1,
                0,
                2,
                1,
                0
            }, "support"),
            new Hero("Wrecking Ball", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                1
            }, "tank"),
            new Hero("Ashe", new int[]
            {
                0,
                1,
                1,
                1,
                0,
                1
            }, "damage"),
            new Hero("Baptiste", new int[]
            {
                0,
                1,
                0,
                1,
                2,
                1
            }, "support"),
            new Hero("Sigma", new int[]
            {
                2,
                1,
                0,
                1,
                2,
                0
            }, "tank")
        };
        internal static int[][] HERO_STAT_POSITIONS = {
            new int[]{ 1030, 894},
            new int[]{ 1030, 957},
            new int[]{ 1300, 894},
            new int[]{ 1300, 957},
            new int[]{ 1570, 894},
            new int[]{ 1570, 957}
        };
        internal static Bitmap[] RANK_LIST = {
            new Bitmap(Resources.Unranked),
            new Bitmap(Resources.Silver),
            new Bitmap(Resources.Gold),
            new Bitmap(Resources.Platinum),
            new Bitmap(Resources.Diamond),
            new Bitmap(Resources.Master),
            new Bitmap(Resources.Grandmaster) };
    }
    public class Hero
    {
        public string name;
        public int[] statSettings;
        public string role;
        public Hero(string name, int[] statSettings, string role)
        {
            this.name = name;
            this.statSettings = statSettings;
            this.role = role;
        }
    }
}
