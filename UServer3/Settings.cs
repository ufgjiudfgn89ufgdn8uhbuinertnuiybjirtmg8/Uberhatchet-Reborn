using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SapphireEngine;

namespace UServer3
{
    public class Settings
    {
        #region [Settings] Connection

        public static SettingsFile FileSettings { get; private set; }

        #endregion


        #region [Settings] Other

        // Если вы падаете с большой высоты, то чит уменьшает урон от падения до 1 HP 
        public static bool SmallFallDamage = false;
        
        // Друзья, с которыми вы играете
        public static HashSet<UInt64> Friends = new HashSet<UInt64>();

        #endregion
        

        #region [Settings] Methods

        public static bool IsFriend(UInt64 steamid) => Friends.Contains(steamid);

        public static void Init()
        {
            if (File.Exists("./settings.json"))
            {
                try
                {
                    FileSettings = JsonConvert.DeserializeObject<SettingsFile>(File.ReadAllText("./settings.json"));
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError("Ошибка чтения конфига, конфиг сброжен на дефолтный!");
                    File.WriteAllText("./settings.json", JsonConvert.SerializeObject(new SettingsFile(), Formatting.Indented));
                }
            }
            else
            {
                ConsoleSystem.LogError("Файл конфига не найден, конфиг сброжен на дефолтный!");
                File.WriteAllText("./settings.json", JsonConvert.SerializeObject(new SettingsFile(), Formatting.Indented));
            }
        }
        
        #endregion
    }

    public class SettingsFile
    {
        [JsonProperty("ConnectionIP")]
        public String ConnectionIP { get; set; } = "37.230.162.86";

        [JsonProperty("ConnectionPort")]
        public Int32 ConnectionPort { get; set; } = 20900;

        [JsonProperty("FakeSteamID")]
        public UInt64 FakeSteamID { get; set; }

        [JsonProperty("FakeUsername")]
        public String FakeUsername { get; set; } = "";
    }
}