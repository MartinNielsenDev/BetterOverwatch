﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using BetterOverwatch.Game;
using BetterOverwatch.DataObjects;
using BetterOverwatch.Properties;

namespace BetterOverwatch
{
    internal class Vars
    {
        public static string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "overwatchtracker");
        public static SoundPlayer successSound = new SoundPlayer(Resources.success);
        public static GameData gameData;
        public static Settings settings;
        public static Initalize initalize;
        public static string lastGameJSON = "";
        public static bool isAdmin = false;
        public static bool overwatchRunning = false;
        public static int blizzardAppOffset = 0;
        public static int loopDelay = 250;
        public static Stopwatch 
            frameTimer = new Stopwatch(),
            getInfoTimeout = new Stopwatch(),
            statsTimer = new Stopwatch();
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

    internal enum Network
    {
        Maps = 0,
        TeamSkillRating = 1,
        Numbers = 2,
        HeroNames = 3,
        PlayerNames = 4
    }
}
