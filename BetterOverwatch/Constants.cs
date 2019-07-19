using BetterOverwatch.Properties;
using System.Drawing;

namespace BetterOverwatch
{
    class Constants
    {
        public static string[] mapList = { "Hanamura", "Horizon Lunar Colony", "Temple of Anubis", "Volskaya Industries", "Blizzard World", "Eichenwalde", "Hollywood", "King's Row", "Numbani", "Ilios", "Lijiang Tower", "Nepal", "Oasis", "Dorado", "Junkertown", "Route 66", "Watchpoint Gibraltar", "Rialto", "Busan", "Paris", "Havana" };
        public static string[] heroComparerList = { "GGGMFIST", "GENJI", "MCCREE", "PHARAH", "REAPER", "SOLDIERZS", "SOMBRA", "TRACER", "BASTION", "HANZG", "JUHKRAT", "MEI", "TDRBJGRN", "WIDOWMAKER", "DVA", "ORISA", "REINHARDT", "RGADHOG", "WINSTON", "ZARYA", "AHA", "BRIGITTE", "LUCIO", "MERCY", "MOIRA", "SYMMETRA", "ZENYATTA", "WRECKIHGBAL", "ASHE", "BAPTISTE" };
        public static Hero[] heroList =
        {
            new Hero("Doomfist", new int[]
            {
                0,
                2,
                0,
                1,
                1,
                2
            }),
            new Hero("Genji", new int[]
            {
                0,
                2,
                0,
                1,
                1,
                0
            }),
            new Hero("McCree", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                1
            }),
            new Hero("Pharah", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Reaper", new int[]
            {
                0,
                1,
                0,
                2,
                1,
                0
            }),
            new Hero("Soldier: 76", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Sombra", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Tracer", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Bastion", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                2
            }),
            new Hero("Hanzo", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                1
            }),
            new Hero("Junkrat", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Mei", new int[]
            {
                2,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Torbjörn", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                2
            }),
            new Hero("Widowmaker", new int[]
            {
                1,
                0,
                0,
                1,
                1,
                0
            }),
            new Hero("D.va", new int[]
            {
                0,
                1,
                0,
                1,
                2,
                0
            }),
            new Hero("Orisa", new int[]
            {
                0,
                1,
                0,
                2,
                2,
                0
            }),
            new Hero("Reinhardt", new int[]
            {
                2,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Roadhog", new int[]
            {
                0,
                0,
                0,
                2,
                1,
                1
            }),
            new Hero("Winston", new int[]
            {
                2,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Zarya", new int[]
            {
                2,
                0,
                0,
                1,
                1,
                0
            }),
            new Hero("Ana", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Brigitte", new int[]
            {
                1,
                1,
                1,
                0,
                2,
                0
            }),
            new Hero("Lúcio", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                0
            }),
            new Hero("Mercy", new int[]
            {
                1,
                1,
                1,
                2,
                1,
                0
            }),
            new Hero("Moira", new int[]
            {
                0,
                1,
                0,
                2,
                1,
                2
            }),
            new Hero("Symmetra", new int[]
            {
                1,
                1,
                0,
                0,
                2,
                0
            }),
            new Hero("Zenyatta", new int[]
            {
                0,
                1,
                0,
                2,
                1,
                0
            }),
            new Hero("Wrecking Ball", new int[]
            {
                0,
                1,
                0,
                1,
                1,
                1
            }),
            new Hero("Ashe", new int[]
            {
                0,
                1,
                1,
                1,
                0,
                1
            }),
            new Hero("Baptiste", new int[]
            {
                0,
                1,
                0,
                1,
                2,
                1
            })
        };
        public static int[][] heroStatsPositions = {
            new int[]{ 1030, 894},
            new int[]{ 1030, 957},
            new int[]{ 1300, 894},
            new int[]{ 1300, 957},
            new int[]{ 1570, 894},
            new int[]{ 1570, 957}
        };
        public static Bitmap[] rankList = {
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
        public Hero(string name, int[] statSetting)
        {
            this.name = name;
            this.statSettings = statSetting;
        }
    }
}
