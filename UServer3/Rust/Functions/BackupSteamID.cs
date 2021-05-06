using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace UServer3.Rust.Functions
{
    public class BackupSteamID
    {
        private const string CONST_FILENAME_STEAMID_LOGS = "steamid_log.json";
        
        private static readonly HashSet<ulong> ListCachedSteamID = new HashSet<ulong>();
        private static Dictionary<ulong, string> ListSteamID = new Dictionary<ulong, string>();

        public static void Init()
        {
            if (File.Exists("./" + CONST_FILENAME_STEAMID_LOGS))
            {
                try
                {
                    ListSteamID = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(File.ReadAllText("./" + CONST_FILENAME_STEAMID_LOGS));
                    foreach (var item in ListSteamID)
                    {
                        ListCachedSteamID.Add(item.Key);
                    }
                }
                catch
                {
                    
                }
            }
        }

        public static bool Contains(ulong steamid) => ListCachedSteamID.Contains(steamid);

        public static void Add(ulong steamid, string username)
        {
            if (Contains(steamid) == false)
            {
                ListCachedSteamID.Add(steamid);
                ListSteamID.Add(steamid, username);
                File.WriteAllText("./" + CONST_FILENAME_STEAMID_LOGS, JsonConvert.SerializeObject(ListSteamID, Formatting.Indented));
            }
        }
    }
}