using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace OverwatchTracker
{
    class Settings
    {
        [JsonProperty("privateToken")]
        public string privateToken { get; set; } = "";
        [JsonProperty("uploadScreenshot")]
        public bool uploadScreenshot { get; set; } = true;
        [JsonProperty("startWithWindows")]
        public bool startWithWindows { get; set; } = true;
        [JsonProperty("playAudioOnSuccess")]
        public bool playAudioOnSuccess { get; set; } = true;

        public static void Load()
        {
            Vars.mapsNeuralNetwork.LoadFromArray(Vars.mapsNeuralNetworkData);
            Vars.digitsNeuralNetwork.LoadFromArray(Vars.digitsNeuralNetworkData);
            Vars.mainMenuNeuralNetwork.LoadFromArray(Vars.mainMenuNeuralNetworkData);
            Vars.blizzardNeuralNetwork.LoadFromArray(Vars.blizzardNeuralNetworkData);
            Vars.heroNamesNeuralNetwork.LoadFromArray(Vars.heroNamesNeuralNetworkData);
            Functions.setVolume(10);

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
            }
            catch
            {

            }
        }
        public static void Save()
        {
            string json = JsonConvert.SerializeObject(Vars.settings, Formatting.Indented);
            File.WriteAllText(Path.Combine(Vars.configPath, "settings.json"), json);
        }
        public static bool FetchUserInfo()
        {
            string token = Server.getToken(true);

            if (token.Contains("success"))
            {
                Vars.publicId = token.Replace("success", "");
                Functions.DebugMessage("Retrieved publicId: " + Vars.publicId);
            }
            else
            {
                if(!token.Equals(String.Empty))
                {
                    Functions.DebugMessage("privateToken verified");
                    Vars.settings.privateToken = token;
                    Save();
                }
                else
                {
                    Functions.DebugMessage("SERVER ERROR, message: " + token);
                    MessageBox.Show("SERVER ERROR", "Overwatch Tracker error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return true;
        }
    }
}
