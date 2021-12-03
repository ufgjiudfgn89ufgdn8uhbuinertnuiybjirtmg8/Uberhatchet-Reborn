using System;
using Network;
using ProtoBuf;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;
using UServer3.Environments;
using UServer3.Rust;
using UServer3.Rust.Data;
using UServer3.Rust.Network;
using UServer3.Rust.Struct;
using BaseNetworkable = UServer3.Rust.BaseNetworkable;
using BasePlayer = UServer3.Rust.BasePlayer;

public class AimAssist : IPlugin
{
    public void Loaded()
    {
    }

    public void Unloaded()
    {
    }

    public void OnPlayerTick(PlayerTick playerTick, PlayerTick previousTick)
    {
    }

    public void OnPlayerTick(PlayerTick playerTick)
    {
    }

    public bool Out_NetworkMessage(Message message)
    {
        switch (message.type)
        {
            case Message.Type.RPCMessage:
                return this.OnRpcMessage(message);
        }

        return false;
    }
    
    public enum EHumanBone : UInt32
    {
        Head = 698017942,
        Jaw = 2822582055,
        Body = 827230707
    }

    private bool OnRpcMessage(Message message)
    {
        message.read._position = 1;
        UInt32 uid = message.read.EntityID();
        ERPCMethodUID rpcMethod = (ERPCMethodUID) message.read.UInt32();
        if (rpcMethod == ERPCMethodUID.PlayerAttack)
        {
            using (PlayerAttack playerAttack = ProtoBuf.PlayerAttack.Deserialize(message.read))
            {
                if (playerAttack != null)
                {
                    BasePlayer player = BaseNetworkable.Get<BasePlayer>(playerAttack.attack.hitID);
                    if (player != null)
                    {
                        if (playerAttack.attack.hitBone == (UInt32) EHumanBone.Jaw || playerAttack.attack.hitBone == (UInt32) EHumanBone.Head)
                            return false;
                        
                        playerAttack.attack.hitBone = ((Rand.Int32(0, 100) < 33) ? (UInt32)EHumanBone.Jaw : ((Rand.Int32(0, 100) < 66) ? (UInt32)EHumanBone.Head : (UInt32)EHumanBone.Body));
                        playerAttack.attack.hitMaterialID = 1395914656;
                        playerAttack.attack.hitPositionWorld = player.Position + new Vector3(0, player.GetHeight(), 0);
                        playerAttack.attack.hitPositionLocal = new Vector3(-0.1f, 0, 0.1f);
                        playerAttack.attack.pointEnd = player.Position + new Vector3(0, player.GetHeight(), 0);
                    
                        if (VirtualServer.BaseClient.write.Start())
                        {
                            VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                            VirtualServer.BaseClient.write.UInt32(uid);
                            VirtualServer.BaseClient.write.UInt32((UInt32) rpcMethod);
                            playerAttack.WriteToStream(VirtualServer.BaseClient.write);
                            VirtualServer.BaseClient.write.Send(new SendInfo());
                        }
                        return true;
                    }
                }
            }
        }
        
        if (rpcMethod == ERPCMethodUID.OnProjectileAttack)
        {
            using (PlayerProjectileAttack playerAttack = ProtoBuf.PlayerProjectileAttack.Deserialize(message.read))
            {
                if (playerAttack != null)
                {
                    BasePlayer player = BaseNetworkable.Get<BasePlayer>(playerAttack.playerAttack.attack.hitID);
                    if (player != null)
                    {
                        if (playerAttack.playerAttack.attack.hitBone == (UInt32) EHumanBone.Jaw || playerAttack.playerAttack.attack.hitBone == (UInt32) EHumanBone.Head)
                            return false;
                        
                        playerAttack.playerAttack.attack.hitBone = ((Rand.Int32(0, 100) < 33) ? (UInt32)EHumanBone.Jaw : ((Rand.Int32(0, 100) < 66) ? (UInt32)EHumanBone.Head : (UInt32)EHumanBone.Body));
                        playerAttack.playerAttack.attack.hitMaterialID = 1395914656;
                        playerAttack.playerAttack.attack.hitPositionWorld = player.Position + new Vector3(0, player.GetHeight(), 0);
                        playerAttack.playerAttack.attack.hitPositionLocal = new Vector3(-0.1f, 0, 0.1f);
                        playerAttack.playerAttack.attack.pointEnd = player.Position + new Vector3(0, player.GetHeight(), 0);
                    
                        if (VirtualServer.BaseClient.write.Start())
                        {
                            VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                            VirtualServer.BaseClient.write.UInt32(uid);
                            VirtualServer.BaseClient.write.UInt32((UInt32) rpcMethod);
                            playerAttack.WriteToStream(VirtualServer.BaseClient.write);
                            VirtualServer.BaseClient.write.Send(new SendInfo());
                        }
                        return true;
                    }
                }
            }
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