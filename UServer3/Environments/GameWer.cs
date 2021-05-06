using System.Net;

namespace UServer3.Environments
{
    public class GameWer
    {
        public static void AuthSteamID(ulong steamid)
        {
            new WebClient().DownloadString("http://auth.server.gamewer.ru:9912/player/update?steamid=" + steamid);
        }
    }
}