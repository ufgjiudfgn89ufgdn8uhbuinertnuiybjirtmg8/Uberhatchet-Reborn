using System;
using Network;
using ProtoBuf;
using SapphireEngine;
using UnityEngine;
using UServer3.Rust.Data;
using UServer3.Rust.Network;
using UServer3.Rust.Struct;
using BaseNetworkable = UServer3.Rust.BaseNetworkable;
using BasePlayer = UServer3.Rust.BasePlayer;

public class SetAdminAccess : IPlugin
{

    private Boolean LastStateFlying = false;

    public void Loaded()
    {
    }

    public void Unloaded()
    {
    }

    public void OnPlayerTick(PlayerTick playerTick, PlayerTick previousTick)
    {
        if (BasePlayer.LocalPlayer == null || BasePlayer.LocalPlayer.IsServerAdmin) return;

        if (playerTick.modelState.flying)
        {
            playerTick.modelState.flying = false;

            this.LastStateFlying = true;
        }
        else
        {
            if (this.LastStateFlying == true) previousTick.modelState.flying = true;
            this.LastStateFlying = false;
        }
    }

    public bool Out_NetworkMessage(Message message)
    {
        return false;
    }

    public bool In_NetworkMessage(Message message)
    {
        switch (message.type)
        {
            case Message.Type.ConsoleCommand:
                string command = message.read.String();
                if (command.Contains("noclip") == true || command.Contains("debugcamera") == true || command.Contains("camspeed") == true)
                    return true;
                ConsoleSystem.Log("[SetAdminAccess] Console command from server: " + command);
                break;
        }

        return false;
    }

    public void OnPacketEntityCreate(Entity entityPacket)
    {
    }

    public bool OnPacketEntityUpdate(Entity entityPacket)
    {
        if (entityPacket == null || entityPacket.basePlayer == null) return false;
        BasePlayer player = BaseNetworkable.Get<BasePlayer>(entityPacket);

        if (player == null) return false;

        if (player.IsServerAdmin || BasePlayer.LocalPlayer != player) return false;

        player.SetPlayerFlag(E_PlayerFlags.IsAdmin, true);

        entityPacket.basePlayer.playerFlags = (Int32) player.PlayerFlags;
        entityPacket.basePlayer.userid = VirtualServer.ConnectionInformation.SteamID;

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