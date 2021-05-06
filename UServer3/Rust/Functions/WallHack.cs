using SapphireEngine;
using UnityEngine;
using UServer3.Rust.Network;

namespace UServer3.Rust.Functions
{
    public class WallHack : SapphireType
    {
        private float m_Interval = 0f;

        public override void OnUpdate()
        {
            if (BasePlayer.LocalPlayer?.CanInteract() == true)
            {
                m_Interval += DeltaTime;

                // Every 0.1s
                if (m_Interval < 0.2f) return;
                m_Interval = 0;
                
                for (var i = 0; i < BasePlayer.ListPlayers.Count; i++)
                {
                    if (BasePlayer.ListPlayers[i] == BasePlayer.LocalPlayer) continue;
                    
                    string text = $"<size=12>" +
                                  $"{BasePlayer.ListPlayers[i].Username} " +
                                  $"[{(int) BasePlayer.ListPlayers[i].Health} hp] " +
                                  $"{(int) Vector3.Distance(BasePlayer.LocalPlayer.Position, BasePlayer.ListPlayers[i].Position)}m" +
                                  $"</size>";
                    Color color = (BasePlayer.ListPlayers[i].IsSleeping ? Color.red : Color.green);
                    
                    if (BasePlayer.ListPlayers[i].IsWounded == true)
                    {
                        color = Color.magenta;
                    }

                    if (BasePlayer.ListPlayers[i].Health == 0)
                    {
                        color = Color.black;
                        text = "<size=10>*</size>";
                    }
                    
                    DDraw.Text(BasePlayer.ListPlayers[i].Position + new Vector3(0, 1.8f, 0), text, color, .2f);
                }
            }
        }
    }
}