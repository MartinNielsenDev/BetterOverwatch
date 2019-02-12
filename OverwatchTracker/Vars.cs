using System;
using System.IO;
using System.Diagnostics;
using System.Media;

namespace BetterOverwatch
{
    class Vars
    {
        public static Game gameData;
        public static Settings settings;
        public static Initalize initalize;
        public static SoundPlayer audio = new SoundPlayer(Properties.Resources.success);
        public static string lastGameJSON = "";
        public static bool isAdmin = false;
        public static int blizzardAppOffset = 0;
        public static string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "overwatchtracker");
        public static bool overwatchRunning = false;
        public static string[] maps = { "Hanamura", "Horizon Lunar Colony", "Temple of Anubis", "Volskaya Industries", "Blizzard World", "Eichenwalde", "Hollywood", "King's Row", "Numbani", "Ilios", "Lijiang Tower", "Nepal", "Oasis", "Dorado", "Junkertown", "Route 66", "Watchpoint Gibraltar", "Rialto", "Busan", "Paris" };
        public static string[] heroNames = { "GGGMFIST", "GENJI", "MCCREE", "PHARAH", "REAPER", "SOLDIERZS", "SOMBRA", "TRACER", "BASTION", "HANZG", "JUHKRAT", "MEI", "TDRBJGRN", "WIDOWMAKER", "DVA", "ORISA", "REINHARDT", "RGADHOG", "WINSTON", "ZARYA", "AHA", "BRIGITTE", "LUCIO", "MERCY", "MOIRA", "SYMMETRA", "ZENYATTA", "WRECKIHGBAL", "ASHE" };
        public static string[] heroNamesReal = { "Doomfist", "Genji", "McCree", "Pharah", "Reaper", "Soldier: 76", "Sombra", "Tracer", "Bastion", "Hanzo", "Junkrat", "Mei", "Torbjörn", "Widowmaker", "D.va", "Orisa", "Reinhardt", "Roadhog", "Winston", "Zarya", "Ana", "Brigitte", "Lúcio", "Mercy", "Moira", "Symmetra", "Zenyatta", "Wrecking Ball", "Ashe" };
        public static int loopDelay = 250;
        public static string[] srCheck = { "", "" }, team1Check = { "", "" }, team2Check = { "", "" }, statsCheck = { "", "", "", "", "" };
        public static int srCheckIndex = 0, team1CheckIndex = 0, team2CheckIndex = 0, statsCheckIndex = 0, roundsCompleted = 0;
        public static Stopwatch frameTimer = new Stopwatch(), gameTimer = new Stopwatch(), roundTimer = new Stopwatch(), heroTimer = new Stopwatch(), getInfoTimeout = new Stopwatch(), statsTimer = new Stopwatch();
    }
    enum State
    {
        Idle = 0,
        Ingame = 1,
        Recording = 2,
        Finished = 3,
        WaitForUpload = 4
    }
    enum Network
    {
        Maps = 0,
        TeamSkillRating = 1,
        Numbers = 2,
        HeroNames = 3
    }
}
