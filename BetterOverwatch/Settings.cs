using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    class Settings
    {
        public string privateToken = "";
        public string publicToken = "";
        public bool uploadScreenshot = true;
        public bool startWithWindows = true;
        public static void Load()
        {
            BetterOverwatchNetworks.Load();
            Functions.SetVolume(10);

            try
            {
                if (File.Exists(Path.Combine(Vars.configPath, "settings.json")))
                {
                    string json = File.ReadAllText(Path.Combine(Vars.configPath, "settings.json"));

                    if (Regex.Replace(json, @"[\s\n\r]", "") != string.Empty && json.Length > 0)
                    {
                        Vars.settings = JsonConvert.DeserializeObject<Settings>(json);
                    }
                }
            }
            catch { }
        }
        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Vars.settings, Formatting.Indented);
            File.WriteAllText(Path.Combine(Vars.configPath, "settings.json"), json);
        }
    }
}
