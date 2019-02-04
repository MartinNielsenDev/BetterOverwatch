using System;
using System.IO;
using System.Diagnostics;
using System.Media;

namespace BetterOverwatch
{
    class Vars
    {
        public static SoundPlayer audio = new SoundPlayer(Properties.Resources.success);
        public static bool isAdmin = false;
        public static int blizzardAppOffset = 0;
        public static string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "overwatchtracker");
        public static GameData gameData;
        public static Settings settings;
        public static Initalize initalize;
        public static bool overwatchRunning = false;
        public static string[] maps = { "Hanamura", "Horizon Lunar Colony", "Temple of Anubis", "Volskaya Industries", "Blizzard World", "Eichenwalde", "Hollywood", "King's Row", "Numbani", "Ilios", "Lijiang Tower", "Nepal", "Oasis", "Dorado", "Junkertown", "Route 66", "Watchpoint Gibraltar", "Rialto", "Busan", "Paris" };
        public static string[] heroNames = { "DOOMFIST", "GENJI", "MCCREE", "PHARAH", "REAPER", "SOLDIERis", "SOMBRA", "TRACER", "BABTION", "HANZO", "JUNKRAT", "MEI", "TORBJORN", "WIDOWMAKER", "DVA", "ORISA", "REINHARDT", "ROGDHOG", "WINSTON", "ZARYA", "ANA", "ARIGITTE", "LUCIO", "MERCY", "MDIRA", "SYMMETRR", "ZENYATTA", "WNECKINGBAL", "AGNE" };
        public static string[] heroNamesReal = { "Doomfist", "Genji", "McCree", "Pharah", "Reaper", "Soldier: 76", "Sombra", "Tracer", "Bastion", "Hanzo", "Junkrat", "Mei", "Torbjörn", "Widowmaker", "D.va", "Orisa", "Reinhardt", "Roadhog", "Winston", "Zarya", "Ana", "Brigitte", "Lúcio", "Mercy", "Moira", "Symmetra", "Zenyatta", "Wrecking Ball", "Ashe" };
        public const int STATUS_IDLE = 0, STATUS_INGAME = 1, STATUS_RECORDING = 2, STATUS_FINISHED = 3, STATUS_WAITFORUPLOAD = 4;
        public static int loopDelay = 250;
        public static string[] srCheck = { "", "" }, team1Check = { "", "" }, team2Check = { "", "" }, statsCheck = { "", "", "", "", "" };
        public static int srCheckIndex = 0, team1CheckIndex = 0, team2CheckIndex = 0, statsCheckIndex = 0, roundsCompleted = 0;
        public static Stopwatch frameTimer = new Stopwatch(), gameTimer = new Stopwatch(), roundTimer = new Stopwatch(), heroTimer = new Stopwatch(), getInfoTimeout = new Stopwatch(), statsTimer = new Stopwatch(), testTimer = new Stopwatch();
    }
}
