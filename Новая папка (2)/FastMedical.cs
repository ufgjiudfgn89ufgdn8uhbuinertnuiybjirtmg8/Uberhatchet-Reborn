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

public class FastMedical : IPlugin
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

    private void SendSelfUse(EEntitySignal signal, string arg)
    {
        if ((BasePlayer.LocalPlayer.ActiveItem?.PrefabID == (UInt32) EPrefabUID.SyringeMedical || BasePlayer.LocalPlayer.ActiveItem?.PrefabID == (UInt32) EPrefabUID.Bandage))
        {
            if (VirtualServer.BaseClient.write.Start())
            {
                VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                VirtualServer.BaseClient.write.UInt32(BasePlayer.LocalPlayer.ActiveItem.UID);
                VirtualServer.BaseClient.write.UInt32((UInt32) ERPCMethodUID.UseSelf);
                VirtualServer.BaseClient.write.Send(new SendInfo());
            }
        }
    }

    private bool OnRpcMessage(Message message)
    {
        message.read._position = 1;
        UInt32 uid = message.read.EntityID();
        ERPCMethodUID rpcMethod = (ERPCMethodUID) message.read.UInt32();
        
        if (rpcMethod == ERPCMethodUID.BroadcastSignalFromClient)
        {
            EEntitySignal signal = (EEntitySignal)message.read.Int32();
            string arg = message.read.String(256);

            if (signal == EEntitySignal.Attack)
            {
                if ((BasePlayer.LocalPlayer.ActiveItem?.PrefabID == (UInt32) EPrefabUID.SyringeMedical || BasePlayer.LocalPlayer.ActiveItem?.PrefabID == (UInt32) EPrefabUID.Bandage))
                {
                    Timer.SetTimeout(() => this.SendSelfUse(signal, arg), 0.5f);
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