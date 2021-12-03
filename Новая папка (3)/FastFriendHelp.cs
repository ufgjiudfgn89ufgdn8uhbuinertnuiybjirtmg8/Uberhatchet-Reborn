using System;
using Network;
using ProtoBuf;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;
using UServer3.Rust;
using UServer3.Rust.Data;
using UServer3.Rust.Network;
using UServer3.Rust.Struct;
using BaseNetworkable = UServer3.Rust.BaseNetworkable;
using BasePlayer = UServer3.Rust.BasePlayer;

public class FastFriendHelp : IPlugin
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

    private bool OnRpcMessage(Message message)
    {
        message.read._position = 1;
        UInt32 uid = message.read.EntityID();
        ERPCMethodUID rpcMethod = (ERPCMethodUID) message.read.UInt32();

        if (rpcMethod == ERPCMethodUID.KeepAlive)
        {
            Timer.SetTimeout(() =>
            {
                if (VirtualServer.BaseClient.write.Start())
                {
                    VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                    VirtualServer.BaseClient.write.UInt32(uid);
                    VirtualServer.BaseClient.write.UInt32((UInt32) ERPCMethodUID.Assist);
                    VirtualServer.BaseClient.write.Send(new SendInfo());
                }
            }, 0.1f);
            return false;
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