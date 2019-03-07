﻿using System;
using System.IO;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    class Settings
    {
        [JsonProperty("privateToken")]
        public string privateToken = "";
        [JsonProperty("publicToken")]
        public string publicToken = "";
        [JsonProperty("uploadScreenshot")]
        public bool uploadScreenshot = true;
        [JsonProperty("startWithWindows")]
        public bool startWithWindows = true;

        public static void Load()
        {
            BetterOverwatchNetworks.Load();
            Functions.SetVolume(10);

            try
            {
                string json;
                if (File.Exists(Path.Combine(Vars.configPath, "settings.json")))
                {
                    json = File.ReadAllText(Path.Combine(Vars.configPath, "settings.json"));
                    if ((json.Replace("\r", String.Empty).Replace("\n", String.Empty) != String.Empty) && json.Length > 0)
                    {
                        Vars.settings = JsonConvert.DeserializeObject<Settings>(json);
                    }
                }
            } catch { }
        }
        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Vars.settings, Formatting.Indented);
            File.WriteAllText(Path.Combine(Vars.configPath, "settings.json"), json);
        }
    }
}
