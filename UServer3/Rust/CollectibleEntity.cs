using System;
using System.Collections.Generic;
using Network;
using ProtoBuf;
using UServer3.Rust.Network;
using UServer3.Rust.Data;

namespace UServer3.Rust
{
    public class CollectibleEntity : BaseEntity
    {
        public static List<CollectibleEntity> ListCollectibles = new List<CollectibleEntity>();

        public override void OnEntityCreate(Entity entity)
        {
            base.OnEntityCreate(entity);
            ListCollectibles.Add(this);
        }

        public override void OnEntityDestroy()
        {
            base.OnEntityDestroy();
            ListCollectibles.Remove(this);
        }
        
        public void PickUp()
        {
            if (VirtualServer.BaseServer.write.Start())
            {
                VirtualServer.BaseServer.write.PacketID(Message.Type.RPCMessage);
                VirtualServer.BaseServer.write.EntityID(this.UID);
                VirtualServer.BaseServer.write.UInt32((UInt32)ERPCMethodUID.Pickup);
                VirtualServer.BaseServer.write.Send(new SendInfo(VirtualServer.BaseServer.connections[0]));
            }
        }
    }
}