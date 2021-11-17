using System;
using System.Collections.Generic;
using System.IO;

namespace UServer3.Rust.Data
{
    public class StringPool
    {
        internal static Dictionary<UInt32, String> HashToString = new Dictionary<uint, string>();
        
        internal static void Init()
        {
            string path = Path.Combine(Bootstrap.DatabasePath, "StringPool.txt");
            if (File.Exists(path))
            {
                string[] stringPoolText = File.ReadAllLines(path);
                for (var i = 0; i < stringPoolText.Length; i++)
                {
                    if (stringPoolText[i].Length > 0)
                    {
                        string key = stringPoolText[i].Split('=')[0];
                        string value = stringPoolText[i].Substring(0, key.Length + 1);
                        uint hash = 0;
                        if (UInt32.TryParse(key, out hash))
                        {
                            HashToString.Add(hash, value);
                        }
                    }
                }
            }
        }

        public static string Get(uint hash)
        {
            if (HashToString.TryGetValue(hash, out string value))
            {
                return value;
            }

            return string.Empty;
        }
    }
}