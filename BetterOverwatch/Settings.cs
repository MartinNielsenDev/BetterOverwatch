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
                if (File.Exists(Path.Combine(AppData.configPath, "settings.json")))
                {
                    string json = File.ReadAllText(Path.Combine(AppData.configPath, "settings.json"));

                    if (Regex.Replace(json, @"[\s\n\r]", "") != string.Empty && json.Length > 0)
                    {
                        AppData.settings = JsonConvert.DeserializeObject<Settings>(json);
                    }
                }
            }
            catch { }
        }
        public static void Save()
        {
            string json = JsonConvert.SerializeObject(AppData.settings, Formatting.Indented);
            File.WriteAllText(Path.Combine(AppData.configPath, "settings.json"), json);
        }
    }
}
