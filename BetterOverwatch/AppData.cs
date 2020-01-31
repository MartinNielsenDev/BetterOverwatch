using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using BetterOverwatch.Game;
using BetterOverwatch.DataObjects;
using BetterOverwatch.Properties;
using ConvNeuralNetwork;

namespace BetterOverwatch
{
    internal class AppData
    {
        public static string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "overwatchtracker");
        public static SoundPlayer successSound = new SoundPlayer(Resources.success_new);
        public static ConvNet[] networks;
        public static GameData gameData;
        public static Settings settings;
        public static Initalize initalize;
        public static string lastGameJSON = "";
        public static bool isAdmin = false;
        public static bool overwatchRunning = false;
        public static int blizzardAppOffset = 0;
        public static int loopDelay = 250;
        public static int win = 0, loss = 0, draw = 0;
        public static Stopwatch 
            frameTimer = new Stopwatch(),
            getInfoTimeout = new Stopwatch(),
            statsTimer = new Stopwatch(),
            ratingsTimer = new Stopwatch();
    }
    internal enum State
    {
        Idle = 0,
        Ingame = 1,
        Recording = 2,
        RoundComplete = 3,
        RoundBeginning = 4,
        Finished = 5,
        WaitForUpload = 6
    }
    internal enum NetworkEnum
    {
        Maps = 0,
        Ratings = 1,
        Stats = 2,
        Players = 3,
        Heroes = 4
    }
}
