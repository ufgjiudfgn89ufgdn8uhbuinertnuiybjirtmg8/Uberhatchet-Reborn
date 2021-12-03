using System;
using System.Collections.Generic;
using Network;
using ProtoBuf;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;
using UServer3.Rust.Data;
using UServer3.Rust.Network;
using UServer3.Rust.Struct;
using BaseNetworkable = UServer3.Rust.BaseNetworkable;
using BasePlayer = UServer3.Rust.BasePlayer;

public class AutoMeleAttack : IPlugin
{
    private bool Enabled = false;
    private BasePlayer TargetPlayer = null;
    private Timer AutoTimer = null;
    private DateTime LastAttackTime = DateTime.MinValue;
    
    #region [Dictionary] ListMeleeMaxDistance
    private static Dictionary<EPrefabUID, float> ListMeleeMaxDistance = new Dictionary<EPrefabUID, float>()
    {
        [EPrefabUID.BoneClub] = 3.6f,
        [EPrefabUID.BoneKnife] = 3.6f,
        [EPrefabUID.KnifeCombat] = 3.6f,
        [EPrefabUID.Hatchet] = 3.6f,
        [EPrefabUID.LongSword] = 3.6f,
        [EPrefabUID.Machete] = 3.6f,
        [EPrefabUID.PixAxe] = 3.6f,
        [EPrefabUID.Rock] = 3.2f,
        [EPrefabUID.SalvagedCleaver] = 3.6f,
        [EPrefabUID.SalvagedHatchet] = 3.6f,
        [EPrefabUID.SalvagedPixAxe] = 3.6f,
        [EPrefabUID.SalvagedSword] = 3.6f,
        [EPrefabUID.StoneHatchet] = 3.6f,
        [EPrefabUID.StonePixAxe] = 3.6f,
        [EPrefabUID.StoneSpear] = 5.5f,
        [EPrefabUID.WoodenSpear] = 5.5f,
    };
    #endregion
    
    #region [Dictionary] ListMeleeHeldSpeed
    private static Dictionary<EPrefabUID, float> ListMeleeHeldSpeed = new Dictionary<EPrefabUID, float>()
    {
        [EPrefabUID.BoneClub] = 0.70f,
        [EPrefabUID.KnifeCombat] = 0.35f,
        [EPrefabUID.BoneKnife] = 0.35f,
        [EPrefabUID.Hatchet] = 0.45f,
        [EPrefabUID.LongSword] = 1f,
        [EPrefabUID.Machete] = 0.65f,
        [EPrefabUID.PixAxe] = 0.75f,
        [EPrefabUID.Rock] = 0.65f,
        [EPrefabUID.SalvagedCleaver] = 1f,
        [EPrefabUID.SalvagedHatchet] = 0.65f,
        [EPrefabUID.SalvagedPixAxe] = 0.65f,
        [EPrefabUID.SalvagedSword] = 0.65f,
        [EPrefabUID.StoneHatchet] = 0.45f,
        [EPrefabUID.StonePixAxe] = 0.45f,
        [EPrefabUID.StoneSpear] = 0.75f,
        [EPrefabUID.WoodenSpear] = 0.75f,
    };
    #endregion
    
    public void Loaded()
    {
        this.AutoTimer = Timer.SetInterval(() =>
        {
            float distance = 4;
            BasePlayer foundPlayer = null;
            BasePlayer.ListPlayers.ForEach(player =>
            {
                float currentDist = Vector3.Distance(player.Position, BasePlayer.LocalPlayer.Position);
                if (player != BasePlayer.LocalPlayer && currentDist <= 3.1f && currentDist < distance)
                {
                    distance = currentDist;
                    foundPlayer = player;
                }
            });
            TargetPlayer = foundPlayer;

            if (TargetPlayer != null && BasePlayer.LocalPlayer.HasActiveItem && ListMeleeHeldSpeed.TryGetValue((EPrefabUID)BasePlayer.LocalPlayer.ActiveItem.PrefabID, out float needInterval))
            {
                if (DateTime.Now.Subtract(LastAttackTime).TotalSeconds > needInterval && Enabled == true && 
                    TargetPlayer.Username.Contains("sofa") == false && 
                    TargetPlayer.Username.Contains("GoodGame") == false &&  
                    TargetPlayer.Username.Contains("Beysh") == false &&  
                    TargetPlayer.Username.Contains("ladno") == false)
                {
                    LastAttackTime = DateTime.Now;
                    
                    if (VirtualServer.BaseClient.write.Start())
                    {
                        VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                        VirtualServer.BaseClient.write.UInt32(BasePlayer.LocalPlayer.UID);
                        VirtualServer.BaseClient.write.UInt32((uint)ERPCMethodUID.BroadcastSignalFromClient);
                        VirtualServer.BaseClient.write.UInt32((uint)EEntitySignal.Attack);
                        VirtualServer.BaseClient.write.Send(new SendInfo());
                    }
                    
                    Console.WriteLine("Mele Attack!");
                    PlayerAttack attack = new PlayerAttack
                    {
                        attack = new Attack
                        {
                            hitItem = BasePlayer.LocalPlayer.ActiveItem.UID,
                            hitID = TargetPlayer.UID,
                            hitBone = 698017942, 
                            hitMaterialID = 1395914656,
                            pointStart = BasePlayer.LocalPlayer.Position + new Vector3(0, BasePlayer.LocalPlayer.GetHeight()),
                            pointEnd = TargetPlayer.Position + new Vector3(0, TargetPlayer.GetHeight()),
                            hitPositionWorld = TargetPlayer.Position + new Vector3(0, TargetPlayer.GetHeight()),
                            hitPositionLocal = new Vector3(-0.1f, 0, 0.1f),
                            hitNormalLocal = new Vector3(-0.1f, 0, 0.1f),
                            hitNormalWorld = new Vector3(-0.1f, 0, 0.1f),
                            hitPartID = 0,
                            ShouldPool = false
                        },
                        projectileID = 0,
                        ShouldPool = false
                    };
                    if (VirtualServer.BaseClient.write.Start())
                    {
                        VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                        VirtualServer.BaseClient.write.UInt32(BasePlayer.LocalPlayer.ActiveItem.UID);
                        VirtualServer.BaseClient.write.UInt32((uint)ERPCMethodUID.PlayerAttack);
                        attack.WriteToStream(VirtualServer.BaseClient.write);
                        VirtualServer.BaseClient.write.Send(new SendInfo());
                    }
                }
            }
        }, 0.1f);
    }

    public void Unloaded()
    {
        this.AutoTimer.Dispose();
    }

    public void OnPlayerTick(PlayerTick playerTick, PlayerTick previousTick)
    {
        
    }

    public bool Out_NetworkMessage(Message message)
    {
        message.read._position = 1; 
        switch (message.type)
        {
            case Message.Type.ConsoleCommand:
            
                string command = message.read.String();

                if (command.Contains("am_enabled") == true)
                {
                    if (command.Contains("1"))
                    {
                        Console.WriteLine("[AutoMeleAttack] Enabled");
                        this.Enabled = true;
                    }
                    else
                    {
                        Console.WriteLine("[AutoMeleAttack] Disabled");
                        this.Enabled = false;
                    }
                    return true;
                }
                break;
        }
        return false;
    }

    public bool In_NetworkMessage(Message message)
    {
        

        return false;
    }

    public void OnPacketEntityCreate(Entity entityPacket)
    {
    }

    public bool OnPacketEntityUpdate(Entity entityPacket)
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
}