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

public class AutoDoorSubmit : IPlugin
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

    Vector3 LastKnkockDoorPosition = Vector3.zero;

    public bool Out_NetworkMessage(Message message)
    {
        message.read._position = 1;
        switch (message.type)
        {
            case Message.Type.RPCMessage:
                message.read._position = 1;
                UInt32 uid = message.read.EntityID();
                uint rpcMethod = message.read.UInt32();
                if (rpcMethod == 1487779344)
                {
                    LastKnkockDoorPosition = BasePlayer.LocalPlayer.Position;
                    Timer knockTimer = null;
                    knockTimer = Timer.SetInterval(() =>
                    {
                        if (Vector3.Distance(LastKnkockDoorPosition, BasePlayer.LocalPlayer.Position) > 3f)
                        {
                            knockTimer.Dispose();
                            return;
                        }

                        if (VirtualServer.BaseClient.write.Start())
                        {
                            VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                            VirtualServer.BaseClient.write.UInt32(uid);
                            VirtualServer.BaseClient.write.UInt32(rpcMethod);
                            VirtualServer.BaseClient.write.Send(new SendInfo());
                        } 
                    }, 0.5f);
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