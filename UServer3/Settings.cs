using System;
using System.Collections.Generic;
using UnityEngine;

namespace UServer3
{
    public class Settings
    {
        #region [Settings] Connection
        
        // IP Сервера к которому будет происходить коннект
        public static String TargetServer_IP = "37.230.162.86";
        
        // Port Сервера к которому будет происходить коннект:
        public static Int32 TargetServer_Port = 20900; //12000

        #endregion


        #region [Settings] Other

        // Если вы падаете с большой высоты, то чит уменьшает урон от падения до 1 HP 
        public static bool SmallFallDamage = false;
        
        // Друзья, с которыми вы играете
        public static HashSet<UInt64> Friends = new HashSet<UInt64>();

        #endregion
        

        #region [Settings] Methods

        public static bool IsFriend(UInt64 steamid) => Friends.Contains(steamid);
        
        #endregion
    }
}