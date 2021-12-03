using System;
using Network;
using ProtoBuf;
using SapphireEngine;
using SapphireEngine.Functions;
using UnityEngine;
using UServer3.Environments;
using UServer3.Rust;
using UServer3.Rust.Data;
using UServer3.Rust.Network;
using UServer3.Rust.Struct;
using BaseNetworkable = UServer3.Rust.BaseNetworkable;
using BasePlayer = UServer3.Rust.BasePlayer;

public class AimAssist : IPlugin
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

    public bool HasMeelee(uint weaponPrefab)
    {
        EPrefabUID weapon = (EPrefabUID) weaponPrefab;
        switch (weapon)
        {
            case EPrefabUID.Chainsaw:
            case EPrefabUID.Hatchet:
            case EPrefabUID.WoodenSpear:
            case EPrefabUID.StoneSpear:
            case EPrefabUID.Machete:
            case EPrefabUID.LongSword:
            case EPrefabUID.SalvagedSword:
            case EPrefabUID.SalvagedCleaver:
            case EPrefabUID.BoneKnife:
            case EPrefabUID.BoneClub:
            case EPrefabUID.Rock:
            case EPrefabUID.SalvagedHatchet:
            case EPrefabUID.StonePixAxe:
            case EPrefabUID.StoneHatchet:
            case EPrefabUID.PixAxe:
            case EPrefabUID.SalvagedPixAxe:
            case EPrefabUID.SalvagedHummer:
            case EPrefabUID.Torch:
            case EPrefabUID.Knife:
            case EPrefabUID.KnifeCombat:
            case EPrefabUID.JackHammer:
            case EPrefabUID.Mace:
                return true;
        }

        return false;
    }

    public bool HasRange(uint weaponPrefab)
    {
        EPrefabUID weapon = (EPrefabUID) weaponPrefab;
        switch (weapon)
        {
            case EPrefabUID.Bow:
            case EPrefabUID.CrossBow:
            case EPrefabUID.CompoundBow:
            case EPrefabUID.NailGun:
            case EPrefabUID.LR300:
            case EPrefabUID.Bolt:
            case EPrefabUID.AK47:
            case EPrefabUID.SemiRifle:
            case EPrefabUID.Pyton:
            case EPrefabUID.Revolver:
            case EPrefabUID.MP5:
            case EPrefabUID.P90:
            case EPrefabUID.DoubleShotgun:
            case EPrefabUID.Tomphson:
            case EPrefabUID.PumpShotgun:
            case EPrefabUID.M249:
            case EPrefabUID.Shotgun:
            case EPrefabUID.Eoka:
            case EPrefabUID.SMG:
            case EPrefabUID.Spas:
            case EPrefabUID.L96:
            case EPrefabUID.M39:
            case EPrefabUID.Bereta:
                return true;
        }

        return false;
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

    public enum EHumanBone : UInt32
    {
        Head = 698017942,
        Jaw = 2822582055,
        Body = 827230707
    }

    private bool OnRpcMessage(Message message)
    {
        message.read._position = 1;
        UInt32 uid = message.read.EntityID();
        ERPCMethodUID rpcMethod = (ERPCMethodUID) message.read.UInt32();
        if (rpcMethod == ERPCMethodUID.PlayerAttack)
        {
            using (PlayerAttack playerAttack = ProtoBuf.PlayerAttack.Deserialize(message.read))
            {
                if (playerAttack != null)
                {
                    BasePlayer player = BaseNetworkable.Get<BasePlayer>(playerAttack.attack.hitID);
                    if (player != null)
                    {
                        if (playerAttack.attack.hitBone == (UInt32) EHumanBone.Jaw || playerAttack.attack.hitBone == (UInt32) EHumanBone.Head)
                            return false;

                        playerAttack.attack.hitBone = ((Rand.Int32(0, 100) < 33) ? (UInt32) EHumanBone.Jaw : ((Rand.Int32(0, 100) < 66) ? (UInt32) EHumanBone.Head : (UInt32) EHumanBone.Body));
                        playerAttack.attack.hitMaterialID = 1395914656;
                        playerAttack.attack.hitPositionWorld = player.Position + new Vector3(0, player.GetHeight(), 0);
                        playerAttack.attack.hitPositionLocal = new Vector3(-0.1f, 0, 0.1f);
                        playerAttack.attack.pointEnd = player.Position + new Vector3(0, player.GetHeight(), 0);

                        if (VirtualServer.BaseClient.write.Start())
                        {
                            VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                            VirtualServer.BaseClient.write.UInt32(uid);
                            VirtualServer.BaseClient.write.UInt32((UInt32) rpcMethod);
                            playerAttack.WriteToStream(VirtualServer.BaseClient.write);
                            VirtualServer.BaseClient.write.Send(new SendInfo());
                        }

                        return true;
                    }
                }
            }
        }

        if (rpcMethod == ERPCMethodUID.OnProjectileAttack)
        {
            using (PlayerProjectileAttack playerAttack = ProtoBuf.PlayerProjectileAttack.Deserialize(message.read))
            {
                if (playerAttack != null)
                {
                    BasePlayer player = BaseNetworkable.Get<BasePlayer>(playerAttack.playerAttack.attack.hitID);
                    if (player != null)
                    {
                        if (playerAttack.playerAttack.attack.hitBone == (UInt32) EHumanBone.Jaw || playerAttack.playerAttack.attack.hitBone == (UInt32) EHumanBone.Head)
                            return false;

                        playerAttack.playerAttack.attack.hitBone = ((Rand.Int32(0, 100) < 33) ? (UInt32) EHumanBone.Jaw : ((Rand.Int32(0, 100) < 66) ? (UInt32) EHumanBone.Head : (UInt32) EHumanBone.Body));
                        playerAttack.playerAttack.attack.hitMaterialID = 1395914656;
                        playerAttack.playerAttack.attack.hitPositionWorld = player.Position + new Vector3(0, player.GetHeight(), 0);
                        playerAttack.playerAttack.attack.hitPositionLocal = new Vector3(-0.1f, 0, 0.1f);
                        playerAttack.playerAttack.attack.pointEnd = player.Position + new Vector3(0, player.GetHeight(), 0);

                        if (VirtualServer.BaseClient.write.Start())
                        {
                            VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
                            VirtualServer.BaseClient.write.UInt32(uid);
                            VirtualServer.BaseClient.write.UInt32((UInt32) rpcMethod);
                            playerAttack.WriteToStream(VirtualServer.BaseClient.write);
                            VirtualServer.BaseClient.write.Send(new SendInfo());
                        }

                        return true;
                    }
                }
            }
        }

        if (rpcMethod == ERPCMethodUID.BroadcastSignalFromClient)
        {
			return false;
            EEntitySignal signal = (EEntitySignal) message.read.Int32();
            string arg = message.read.String(256);

            if (signal == EEntitySignal.Attack && BasePlayer.LocalPlayer.HasActiveItem)
            {
                if (OpCodes.IsMeleeWeapon_Prefab((EPrefabUID)BasePlayer.LocalPlayer.ActiveItem.PrefabID))
                {
                    Timer.SetTimeout(() =>
                    {
                        if (BasePlayer.LocalPlayer.HasActiveItem && OpCodes.IsMeleeWeapon_Prefab((EPrefabUID) BasePlayer.LocalPlayer.ActiveItem.PrefabID))
                        {
                            var targetPlayer = this.GetMeeleePlayer();

                            var bone = OpCodes.GetTargetHit(0, true);
                            var attackInfo = OpCodes.GetTargetHitInfo(bone);

                            var closestPoint = targetPlayer.ClosestPoint(BasePlayer.LocalPlayer.EyePos);
                            var offset = BasePlayer.LocalPlayer.GetForward(closestPoint) * 1.4f;
                            DDraw.DrawBox(targetPlayer.WorldSpaceBounds().position, targetPlayer.WorldSpaceBounds().extents * 2, Color.magenta, 1f);
                            DDraw.Arrow(closestPoint, closestPoint - offset, 0.1f, Color.blue, 1f);
                            var position = closestPoint - offset;
                            BasePlayer.LocalPlayer.ActiveItem.SendMeleeAttack(targetPlayer, bone, position);
                        }
                    }, 0.1f);
                }

                if (this.HasRange(BasePlayer.LocalPlayer.ActiveItem.PrefabID) == true)
                {
                    // BasePlayer targetPlayer = this.GetMeeleePlayer();
                    // PlayerProjectileAttack playerAttack = new PlayerProjectileAttack();
                    // playerAttack.hitDistance = Vector3.Distance(BasePlayer.LocalPlayer.Position, targetPlayer.Position);
                    // playerAttack.hitVelocity = Vector3.Normalize(targetPlayer.Position - BasePlayer.LocalPlayer.Position);
                    // playerAttack.travelTime = 0.7f;
                    // playerAttack.playerAttack = new PlayerAttack();
                    // playerAttack.playerAttack.
                }

                return false;
            }
        }

        return false;
    }

    public BasePlayer GetMeeleePlayer()
    {
        float lastMinDistance = 3.1f;
        BasePlayer result = null;
        for (var i = 0; i < BasePlayer.ListPlayers.Count; i++)
        {
            BasePlayer player = BasePlayer.ListPlayers[i];
            if (player != BasePlayer.LocalPlayer)
            {
                float distance = Vector3.Distance(player.Position, BasePlayer.LocalPlayer.Position);
                if (distance <= 3 && distance < lastMinDistance)
                {
                    result = player;
                }
            }
        }

        return result;
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