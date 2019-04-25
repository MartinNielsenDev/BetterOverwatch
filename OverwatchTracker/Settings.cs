using System.IO;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    internal class Settings
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
                if (File.Exists(Path.Combine(Vars.configPath, "settings.json")))
                {
                    string json = File.ReadAllText(Path.Combine(Vars.configPath, "settings.json"));
                    if ((json.Replace("\r", string.Empty).Replace("\n", string.Empty) != string.Empty) && json.Length > 0)
                    {
                        Vars.settings = JsonConvert.DeserializeObject<Settings>(json);
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Vars.settings, Formatting.Indented);
            File.WriteAllText(Path.Combine(Vars.configPath, "settings.json"), json);
        }
    }
}
