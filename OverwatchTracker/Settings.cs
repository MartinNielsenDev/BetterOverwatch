using System;
using System.IO;
using Newtonsoft.Json;

namespace BetterOverwatch
{
    class Settings
    {
        /// <summary>
        /// Used by the server to authenticate the user on the website, this is essentially a password.
        /// </summary>
        [JsonProperty("privateToken")]
        public string privateToken { get; set; } = "";
        /// <summary>
        /// This will be the user's permanent link on the website, to share their profile with others, this can be changed.
        /// </summary>
        [JsonProperty("publicToken")]
        public string publicToken { get; set; } = "";
        [JsonProperty("uploadScreenshot")]
        public bool uploadScreenshot { get; set; } = true;
        [JsonProperty("startWithWindows")]
        public bool startWithWindows { get; set; } = true;
        [JsonProperty("playAudioOnSuccess")]
        public bool playAudioOnSuccess { get; set; } = true;

        public static void Load()
        {
            OCRNetworkData.LoadOCRNetworkData();
            Functions.SetVolume(10);

            try
            {
                string json;
                if (File.Exists(Path.Combine(Vars.configPath, "settings.json")))
                {
                    Functions.DebugMessage("Loading 'settings.json'");
                    json = File.ReadAllText(Path.Combine(Vars.configPath, "settings.json"));
                    if ((json.Replace("\r", String.Empty).Replace("\n", String.Empty) != String.Empty) && json.Length > 0)
                    {
                        Vars.settings = JsonConvert.DeserializeObject<Settings>(json);
                    }
                }
            }catch { }
        }
        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Vars.settings, Formatting.Indented);
            File.WriteAllText(Path.Combine(Vars.configPath, "settings.json"), json);
        }
        public static bool VerifyUser()
        {
            if (Server.FetchTokens())
            {
                Functions.DebugMessage("privateToken verified");
                Save();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
