using System;
using System.Collections.Generic;
using Network;
using ProtoBuf;
using UnityEngine;
using UServer3.CSharp.ExtensionMethods;
using UServer3.Rust.Network;
using Bounds = UServer3.Rust.Struct.Bounds;
using OBB = UServer3.Rust.Struct.OBB;

namespace UServer3.Rust
{
    public class BaseEnvironment : BaseEntity
    {
        public override void OnEntityCreate(Entity entity)
        {
            base.OnEntityCreate(entity);
        }

        public override void OnEntityUpdate(Entity entity)
        {
            base.OnEntityUpdate(entity);
        }


        public override bool OnEntity(Entity entity)
        {
            entity.environment.dateTime = 5250206760382237147L;
            if (VirtualServer.BaseServer.write.Start())
            {
                VirtualServer.BaseServer.write.PacketID(Message.Type.Entities);
                VirtualServer.BaseServer.write.UInt32(VirtualServer.TakeEntityNUM);
                entity.WriteToStream(VirtualServer.BaseServer.write);
                VirtualServer.BaseServer.write.Send(new SendInfo(VirtualServer.BaseServer.connections[0]));
            }
            
            entity.Dispose();
            return true;
        }
    }
}