using Network;
using SapphireEngine;
using UnityEngine;
using UServer3.Rust;
using UServer3.Rust.Data;
using UServer3.Rust.Network;
using UServer3.Rust.Struct;

public class ESP : IPlugin
{
    private ESPWorker CurrentWorker;

    public void Loaded()
    {
        this.CurrentWorker = Framework.Bootstraper.AddType<ESPWorker>();
    }

    public void Unloaded()
    {
        this.CurrentWorker.Dispose();
    }

    public void OnPlayerTick(PlayerTick playerTick, PlayerTick previousTick)
    {
    }

    public bool Out_NetworkMessage(Message message)
    {
        return false;
    }

    public bool In_NetworkMessage(Message message)
    {
        return false;
    }

    public void OnPacketEntityCreate(ProtoBuf.Entity entityPacket)
    {
    }

    public bool OnPacketEntityUpdate(ProtoBuf.Entity entityPacket)
    {
        return false;
    }

    public void OnPacketEntityPosition(uint uid, Vector3 position, Vector3 rotation)
    {
    }

    public void OnPacketEntityDestroy(uint uid)
    {
    }

    public bool CallHook(string name, object[] args)
    {
        return false;
    }

    class ESPWorker : SapphireType
    {
        private float m_Interval = 0f;

        public override void OnAwake()
        {
        }

        private float GetBonusYFromDistance(float distance)
        {
            float bonus = 0;
            
            if (distance > 70)
            {
                bonus += (distance / 55);
            } else if (distance > 20)
            {
                bonus += (distance / 75);
            }
            return bonus;
        }

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
                    if (BasePlayer.ListPlayers[i].IsDead == true) continue;

                    float distance = Vector3.Distance(BasePlayer.LocalPlayer.Position, BasePlayer.ListPlayers[i].Position);
                    Vector3 position = BasePlayer.ListPlayers[i].Position;

                    float bonus = this.GetBonusYFromDistance(distance);
                    position = position + new Vector3(0, bonus * -1, 0);

                    if (distance > 350) continue;

                    string weapon = "";
                    if (BasePlayer.ListPlayers[i].HasActiveItem)
                    {
                        weapon += $"\n<color=orange>{((EPrefabUID) BasePlayer.ListPlayers[i].ActiveItem.PrefabID).ToString()}</color>";
                    }

                    if (BasePlayer.ListPlayers[i].Belt != null && BasePlayer.ListPlayers[i].Belt.contents != null)
                    {
                        ConsoleSystem.Log(BasePlayer.ListPlayers[i].Belt.ToString());
                        weapon += "\n";
                        for (var j = 0; j < BasePlayer.ListPlayers[i].Belt.contents.Count; j++)
                        {
                            weapon += BasePlayer.ListPlayers[i].Belt.contents[j].name + " |";
                        }
                    }

                    string text = $"<size=12>" +
                                  $"{BasePlayer.ListPlayers[i].Username} " +
                                  $"[{(int) BasePlayer.ListPlayers[i].Health} hp] " +
                                  $"{(int) distance}m" +
                                  weapon +
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

                    DDraw.Text(position, text, color, .2f);
                }
            }
        }
    }
}