using System;
using Network;
using ProtoBuf;
using UnityEngine;
using UServer3.Rust.Data;
using UServer3.Rust.Network;
using UServer3.Rust.Struct;

public class AnyTimeIsDay : IPlugin
{
    private DateTime NeedDateTime = new DateTime(2000, 01, 01, 12, 00, 00);

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
        if (entityPacket.environment == null) return false;
        
        entityPacket.environment.dateTime = NeedDateTime.ToBinary();
        if (VirtualServer.BaseServer.write.Start())
        {
            VirtualServer.BaseServer.write.PacketID(Message.Type.Entities);
            VirtualServer.BaseServer.write.UInt32(VirtualServer.TakeEntityNUM);
            entityPacket.WriteToStream(VirtualServer.BaseServer.write);
            VirtualServer.BaseServer.write.Send(new SendInfo(VirtualServer.BaseServer.connections[0]));
            return true;
        }

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