using Network;
using ProtoBuf;
using UnityEngine;
using UServer3.Rust.Data;

namespace UServer3.Rust.Struct
{
    public interface IPlugin
    {
        void Loaded();
        void Unloaded();
        void OnPlayerTick(PlayerTick playerTick, PlayerTick previousTick);
        bool Out_NetworkMessage(Message message);
        bool In_NetworkMessage(Message message);
        void OnPacketEntityCreate(Entity entityPacket);
        bool OnPacketEntityUpdate(Entity entityPacket);
        void OnPacketEntityPosition(uint uid, Vector3 position, Vector3 rotation);
        void OnPacketEntityDestroy(uint uid);
        bool CallHook(string name, object[] args);
    }
}