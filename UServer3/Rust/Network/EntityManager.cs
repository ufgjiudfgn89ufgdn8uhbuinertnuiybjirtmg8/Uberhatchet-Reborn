using System;
using Network;
using ProtoBuf;
using SapphireEngine;
using UnityEngine;
using UServer3.Environments;
using UServer3.Rust.Data;

namespace UServer3.Rust.Rust
{
    public static class EntityManager
    {
        public static bool OnEntity(Entity entity)
        {
            var ent = BaseNetworkable.Get(entity.baseNetworkable.uid);
            if (ent != null)
            {
                ent.OnEntityUpdate(entity);

                bool returnResult2 = false;
                bool result2 = ent.OnEntity(entity);

                if (result2 == true)
                {
                    returnResult2 = result2;
                }
                bool resultUpdate2 = PluginManager.Instance.CallHook_OnPacketEntityUpdate(entity);
                if (resultUpdate2 == true)
                {
                    returnResult2 = resultUpdate2;
                }

                return returnResult2;
            }

            var prefabId = entity.baseNetworkable.prefabID;

            if (prefabId == (UInt32) EPrefabUID.BasePlayer)
            {
                ent = new BasePlayer();
            }
            else if (prefabId == (UInt32) EPrefabUID.OreBonus)
            {
                ent = new OreBonus();
            }
            else if (entity.resource != null && Database.IsOreResource(prefabId))
            {
                ent = new OreResource();
            }
            else if (entity.heldEntity != null)
            {
                ent = new BaseHeldEntity();
            }
            else if (OpCodes.IsStorage(entity.baseNetworkable.prefabID))
            {
                ent = new StorageContainer();
            }
            else if (Database.IsCollectible(prefabId))
            {
                ent = new CollectibleEntity();
            }
            else if (Database.IsBaseResource(prefabId))
            {
                ent = new BaseResource();
            }
            else if (entity.worldItem != null && Database.IsComponent(entity.worldItem.item.itemid))
            {
                //new WorldItem();
            }
            else if (entity.environment != null && entity.basePlayer == null)
            {
                ent = new BaseEnvironment();
            }
            else
            {
                ent = new BaseNetworkable();
            }


            ent.OnEntityCreate(entity);
            PluginManager.Instance.CallHook_OnPacketEntityCreate(entity);
            bool returnResult = false;
            bool result = ent.OnEntity(entity);
            if (result == true)
            {
                returnResult = result;
            }

            bool resultUpdate = PluginManager.Instance.CallHook_OnPacketEntityUpdate(entity);
            if (resultUpdate == true)
            {
                returnResult = resultUpdate;
            }

            return returnResult;
        }

        public static void OnEntityDestroy(Message packet)
        {
            UInt32 uid = packet.read.EntityID();
            PluginManager.Instance.CallHook_OnPacketEntityDestroy(uid);
            BaseNetworkable.Destroy(uid);
        }

        public static void OnEntityPosition(Message packet)
        {
            /* EntityPosition packets may contain multiple positions */
            while ((long) packet.read.Unread >= (long) 28)
            {
                uint num = packet.read.EntityID();
                var entity = BaseNetworkable.Get<BaseEntity>(num);
                if (entity != null)
                {
                    Vector3 position = packet.read.Vector3();
                    Vector3 rotation = packet.read.Vector3();

                    entity.OnPositionUpdate(position, rotation);
                    PluginManager.Instance.CallHook_OnPacketEntityPosition(num, position, rotation);
                }
            }
        }

    }
}