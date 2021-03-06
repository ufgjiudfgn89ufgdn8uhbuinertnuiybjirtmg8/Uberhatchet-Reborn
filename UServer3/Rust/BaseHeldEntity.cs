using System;
using System.Collections.Generic;
using Network;
using ProtoBuf;
using SapphireEngine;
using UnityEngine;
using UServer3.CSharp.Reflection;
using UServer3.Rust.Network;
using UServer3.Rust.Data;

namespace UServer3.Rust
{
    public class BaseHeldEntity : BaseEntity
    {
        public UInt32 ItemID;
        public UInt32 Parent;
        public UInt32 Bone;

        public Int32 AmmoType = 0;
        public bool IsProjectile = false;
        public bool IsMelee() => OpCodes.IsMeleeWeapon_Prefab((EPrefabUID)PrefabID);
        
        public override void OnEntityUpdate(Entity entity)
        {
            base.OnEntityUpdate(entity);
            if (entity.baseProjectile != null)
            {
                IsProjectile = true;
                if (entity.baseProjectile.primaryMagazine != null)
                {
                    AmmoType = entity.baseProjectile.primaryMagazine.ammoType;
                }
            }
            if (entity.heldEntity != null)
            {
                ItemID = entity.heldEntity.itemUID;
            }
            if (entity.parent != null)
            {
                Parent = entity.parent.uid;
                Bone = entity.parent.bone;
            }
        }

        #region [RPCMethod] CLProject
        [RPCMethod(ERPCMethodUID.CLProject)]
        private bool RPC_OnCLProject(ERPCNetworkType type, Message message)
        {
            using (ProjectileShoot projectileShoot = ProjectileShoot.Deserialize(message.read))
            {
                foreach (ProjectileShoot.Projectile projectile in projectileShoot.projectiles)
                {
                    //RangeAim.NoteFiredProjectile(projectile.projectileID, PrefabID, AmmoType);
                }
            }
            return false;
        }
        #endregion

        #region [RPCMethod] PlayerAttack
        [RPCMethod(ERPCMethodUID.PlayerAttack)]
        private bool RPC_OnPlayerAttack(ERPCNetworkType type, Message message)
        {
            using (PlayerAttack playerAttack = PlayerAttack.Deserialize(message.read))
            {
                var attack = playerAttack.attack;
                if (attack.hitID == 0) return true;
                
                #region [BaseResource]
                var resource = Get<BaseResource>(attack.hitID);
                if (resource != null)
                {
                    attack.hitItem = 0;
                    attack.hitBone = 0;
                    attack.hitPartID = 0;
                    var pos = resource.Position;
                    
                    // Если это OreResource
                    if (pos != resource.Position)
                    {
                        attack.hitPositionWorld = pos;
                        attack.hitNormalWorld = pos;
                    }
                    
                    if (VirtualServer.BaseClient.write.Start())
                    {
                        VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                        VirtualServer.BaseClient.write.EntityID(this.UID);
                        VirtualServer.BaseClient.write.UInt32((UInt32)ERPCMethodUID.PlayerAttack);
                        PlayerAttack.Serialize(VirtualServer.BaseClient.write, playerAttack);
                        VirtualServer.BaseClient.write.Send(new SendInfo());
                        return true;
                    }
                }
                #endregion
                
                // #region [BasePlayer]
                // if (Settings.Aimbot_Melee_Manual)
                // {
                //     var player = Get<BasePlayer>(playerAttack.attack.hitID);
                //     if (player != null)
                //     {
                //         var typeHit = OpCodes.GetTargetHit((EHumanBone) attack.hitBone, Settings.Aimbot_Melee_Manual_AutoHeadshot);
                //         return SendMeleeAttack(player, typeHit, player.Position);
                //     }
                // }
                // #endregion
            }
            return false;
        }
        #endregion
        
        #region [Method] SendMeleeAttack
        public bool SendMeleeAttack(BaseEntity target, EHumanBone bone, Vector3 position)
        {
            ConsoleSystem.Log(bone.ToString());
            var attackInfo = OpCodes.GetTargetHitInfo(bone);
            PlayerAttack attack = new PlayerAttack()
            {
                projectileID = 0,
                attack = new Attack()
                {
                    hitID = target.UID,
                    hitItem = 0,
                    hitBone = attackInfo.HitBone,
                    hitMaterialID = 97517300,
                    hitPartID = attackInfo.HitPartID,
                    pointEnd = position,
                    pointStart = position,
                    hitPositionLocal = attackInfo.HitLocalPos,
                    hitPositionWorld = position,
                    hitNormalLocal = attackInfo.HitNormalPos,
                    hitNormalWorld = position
                }
            };

            if (VirtualServer.BaseClient.write.Start())
            {
                VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                VirtualServer.BaseClient.write.EntityID(this.UID);
                VirtualServer.BaseClient.write.UInt32((UInt32)ERPCMethodUID.PlayerAttack);
                PlayerAttack.Serialize(VirtualServer.BaseClient.write, attack);
                VirtualServer.BaseClient.write.Send(new SendInfo());
            }
            
            return true;
        }
        #endregion

        #region [Method] SendMeleeResourceAttack
        public void SendMeleeResourceAttack(BaseResource baseResource, bool bonus)
        {
            // Если бонус нам не нужен, то по нему не ударяем
            Vector3 position = bonus ? baseResource.GetHitPosition() : baseResource.Position;
            PlayerAttack attack = new PlayerAttack()
            {
                projectileID = 0,
                attack = new Attack()
                {
                    hitID = baseResource.UID,
                    hitItem = 0,
                    hitBone = 0,
                    hitMaterialID = 97517300,
                    hitPartID = 0,
                    pointEnd = position,
                    pointStart = position,
                    hitPositionLocal = position,
                    hitPositionWorld = position,
                    hitNormalLocal = position,
                    hitNormalWorld = position,
                }
            };
            
            
            if (VirtualServer.BaseClient.write.Start())
            {
                VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                VirtualServer.BaseClient.write.EntityID(this.UID);
                VirtualServer.BaseClient.write.UInt32((UInt32)ERPCMethodUID.PlayerAttack);
                PlayerAttack.Serialize(VirtualServer.BaseClient.write, attack);
                VirtualServer.BaseClient.write.Send(new SendInfo());
            }
        }
        #endregion
    }
}