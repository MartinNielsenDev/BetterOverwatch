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
        internal static string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "overwatchtracker");
        internal static SoundPlayer successSound = new SoundPlayer(Resources.success_new);
        internal static ConvNet[] networks;
        internal static GameData gameData;
        internal static Settings settings;
        internal static Initalize initalize;
        internal static string lastGameJSON = "";
        internal static bool isAdmin = false;
        internal static bool overwatchRunning = false;
        internal static int blizzardAppOffset = 0;
        internal static int loopDelay = 500;
        internal static int win = 0, loss = 0, draw = 0;
        internal static Stopwatch
            infoTimer = new Stopwatch(),
            statsTimer = new Stopwatch(),
            ratingsTimer = new Stopwatch();
    }
    internal enum State
    {
        Idle = 0,
        Ingame = 1,
        Record = 2,
        RoundComplete = 3,
        RoundStart = 4,
        Finished = 5,
        Upload = 6
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
