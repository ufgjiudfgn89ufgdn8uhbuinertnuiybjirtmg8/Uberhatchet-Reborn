using System;
using System.Collections.Generic;
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

public class RangeAim : IPlugin
{
    private static Dictionary<Int32, FiredProjectile> FiredProjectiles = new Dictionary<Int32, FiredProjectile>();
    private static float GetCurrentTime() => (float) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

    public static void NoteFiredProjectile(Int32 projectileID, UInt32 prefabID, Int32 ammotype)
    {
        FiredProjectiles[projectileID] = (new FiredProjectile()
        {
            FiredTime = GetCurrentTime(),
            PrefabID = prefabID,
            AmmoType = ammotype,
        });
    }

    private Timer CurrentPluginTimer;
    private Boolean HasPluginUnloaded = false;

    private float m_interval_update_tick = 0f;
    private float m_no_target_time = 0;

    public BasePlayer TargetPlayer = null;
    private Stack<TargetAimInformation> m_list_players = new Stack<TargetAimInformation>();

    private static Dictionary<Int32, Single> ListMaxVelocity = new Dictionary<Int32, Single>()
    {
        [0] = 25,
        [588596902] = 120, // ammo.handmade.shell
        [785728077] = 300, // ammo.pistol
        [51984655] = 225, // ammo.pistol.fire
        [-1691396643] = 400, // ammo.pistol.hv
        [-1211166256] = 375, // ammo.rifle
        [-1321651331] = 225, // ammo.rifle.explosive
        [605467368] = 225, // ammo.rifle.incendiary
        [1712070256] = 450, // ammo.rifle.hv
        [-1685290200] = 245, // ammo.shotgun
        [-1036635990] = 245, // ammo.shotgun.fire
        [-727717969] = 225, // ammo.shotgun.slug
        [-1023065463] = 80, // arrow.hv
        [-1234735557] = 50, // arrow.wooden
        [-2097376851] = 50, // ammo.nailgun.nails
    };

    private static Dictionary<Int32, Single> ListProjectileInitialDistance = new Dictionary<Int32, Single>()
    {
        [0] = 0,
        [588596902] = 0, // ammo.handmade.shell
        [785728077] = 15, // ammo.pistol
        [51984655] = 15, // ammo.pistol.fire
        [-1691396643] = 15, // ammo.pistol.hv
        [-1211166256] = 15, // ammo.rifle
        [-1321651331] = 15, // ammo.rifle.explosive
        [605467368] = 15, // ammo.rifle.incendiary
        [1712070256] = 15, // ammo.rifle.hv
        [-1685290200] = 3, // ammo.shotgun
        [-1036635990] = 3, // ammo.shotgun.fire
        [-727717969] = 10, // ammo.shotgun.slug
        [-1023065463] = 0, // arrow.hv
        [-1234735557] = 0, // arrow.wooden
        [-2097376851] = 0, // ammo.nailgun.nails
    };

    private static Dictionary<EPrefabUID, Single> ListProjectileVelocityScale = new Dictionary<EPrefabUID, Single>()
    {
        [EPrefabUID.Bow] = 1,
        [EPrefabUID.CrossBow] = 1.5f,
        [EPrefabUID.Revolver] = 1,
        [EPrefabUID.Shotgun] = 1,
        [EPrefabUID.Tomphson] = 1,
        [EPrefabUID.SemiRifle] = 1,
        [EPrefabUID.P90] = 1,
        [EPrefabUID.Pyton] = 1,
        [EPrefabUID.PumpShotgun] = 1,
        [EPrefabUID.NailGun] = 1,
        [EPrefabUID.MP5] = 0.8f,
        [EPrefabUID.Bereta] = 1,
        [EPrefabUID.M249] = 1.3f,
        [EPrefabUID.LR300] = 1,
        [EPrefabUID.Eoka] = 1,
        [EPrefabUID.DoubleShotgun] = 1,
        [EPrefabUID.SMG] = 0.8f,
        [EPrefabUID.AK47] = 1,
        [EPrefabUID.Bolt] = 1.75f,
        [EPrefabUID.L96] = 1.75f,
        [EPrefabUID.M39] = 1.75f,
    };

    private static Dictionary<EHumanBone, HitInfo> ListProjectileHumanHits = new Dictionary<EHumanBone, HitInfo>()
    {
        {
            EHumanBone.Head, new HitInfo
            {
                HitBone = (uint) EHumanBone.Head,
                HitPartID = 0, //1744899316,
                HitLocalPos = new Vector3(-0.1f, -0.1f, 0.0f),
                HitNormalPos = new Vector3(0.0f, -1.0f, 0.0f)
            }
        },
        {
            EHumanBone.Body, new HitInfo
            {
                HitBone = (uint) EHumanBone.Body,
                HitPartID = 0, //1890214305,
                HitLocalPos = new Vector3(0.0f, 0.2f, 0.1f),
                HitNormalPos = new Vector3(0.7f, -0.3f, 0.7f)
            }
        }
    };

    public static HitInfo GetTargetHitInfo(EHumanBone humanBone)
    {
        if (ListProjectileHumanHits.TryGetValue(humanBone, out HitInfo res))
        {
            return res;
        }

        return default(HitInfo);
    }

    public static float GetProjectileVelocityScale(EPrefabUID uid)
    {
        if (ListProjectileVelocityScale.TryGetValue(uid, out Single velocityScale))
        {
            return velocityScale;
        }

        return 0;
    }

    public static float GetProjectileInitialDistance(Int32 id)
    {
        if (ListProjectileInitialDistance.TryGetValue(id, out Single distance))
        {
            return distance;
        }

        return 0;
    }

    public static float GetMaxVelocity(Int32 id)
    {
        if (ListMaxVelocity.TryGetValue(id, out Single velocity))
        {
            return velocity;
        }

        return 225;
    }

    public void Loaded()
    {
        this.CurrentPluginTimer = Timer.SetInterval(() =>
        {
            if (HasPluginUnloaded == true)
            {
                this.CurrentPluginTimer.Dispose();
                return;
            }

            this.OnUpdate();
        }, 0.1f);
    }

    private void OnUpdate()
    {
        this.m_interval_update_tick += 0.1f;
        if (this.m_interval_update_tick >= 0.1f)
        {
            this.m_interval_update_tick = 0f;
            ///
            if (BasePlayer.IsHaveLocalPlayer)
            {
                if (this.m_no_target_time >= 0.5f)
                {
                    this.m_no_target_time = 0f;
                    this.TargetPlayer = null;
                }

                #region [Section] Find Target

                for (int i = 0; i < BasePlayer.ListPlayers.Count; ++i)
                {
                    if (BasePlayer.ListPlayers[i].IsLocalPlayer == false && BasePlayer.ListPlayers[i].IsAlive)
                    {
                        float distance = Vector3.Distance(BasePlayer.ListPlayers[i].Position, BasePlayer.LocalPlayer.Position);
                        if ((BasePlayer.LocalPlayer.HasActiveItem && OpCodes.IsFireWeapon_Prefab((EPrefabUID) BasePlayer.LocalPlayer.ActiveItem.PrefabID) && distance < 150) || distance < 50)
                        {
                            #region [Section] Range and Radius check

                            Vector3 forward = BasePlayer.LocalPlayer.GetForward() * distance + BasePlayer.LocalPlayer.EyePos;
                            float distance_check = 5f;

                            if (distance < 10)
                                distance_check = distance / 2;
                            else if (distance > 30)
                                distance_check = 9;

                            distance_check = 100;
                            float distance_point_and_playuer = Vector3.Distance(forward, BasePlayer.ListPlayers[i].Position + new Vector3(0, BasePlayer.ListPlayers[i].GetHeight() * 0.5f, 0));
                            if (distance_point_and_playuer < distance_check)
                                m_list_players.Push(new TargetAimInformation {Player = BasePlayer.ListPlayers[i], DistanceCursor = distance_point_and_playuer});

                            #endregion
                        }
                    }
                }

                if (this.m_list_players.Count > 0)
                {
                    BasePlayer target = null;
                    float dist = float.MaxValue;
                    while (this.m_list_players.Count > 0)
                    {
                        TargetAimInformation player = this.m_list_players.Pop();
                        if (dist > player.DistanceCursor)
                        {
                            dist = player.DistanceCursor;
                            target = player.Player;
                        }
                    }

                    this.TargetPlayer = target;
                }
                else if (this.TargetPlayer != null)
                    m_no_target_time += 0.1f;

                #endregion

                if (this.TargetPlayer != null)
                    DDraw.Text(this.TargetPlayer.Position + new Vector3(0, this.TargetPlayer.GetHeight(), 0), $"<size=32>.</size>", Color.red, 0.1f);
//                    if (this.TargetPlayer != null)
//                        DDraw.DrawBox(this.TargetPlayer.Position + new Vector3(0, this.TargetPlayer.GetHeight()*0.5f, 0), this.TargetPlayer.Rotation.ToQuaternion(), new Vector3(1,this.TargetPlayer.GetHeight(), 1), Color.red, 0.05f);
            }

            ///
        }
    }

    private static float GetTimeout(FiredProjectile projectile, float distance)
    {
        ConsoleSystem.Log("=> " + projectile.AmmoType);
        double maxVelocity = GetMaxVelocity(projectile.AmmoType);
        if (projectile.AmmoType > 0)
            maxVelocity *= GetProjectileVelocityScale((EPrefabUID) projectile.PrefabID);
        double y = projectile.FiredTime + 1f;
        double z = maxVelocity;
        double w = GetProjectileInitialDistance(projectile.AmmoType);
        double f = distance;
        double chisl = (-w + f + 1.5f * y * z - 0.09799f * z);
        double znam = (1.5f * z);
        double drob = chisl / znam;
        double normDrob = drob - GetCurrentTime();
        return (float) normDrob;
    }

    public bool Silent(PlayerProjectileAttack attack)
    {
        if (this.TargetPlayer != null)
        {
            var hitPosition = this.TargetPlayer.Position;
            var distance = Vector3.Distance(BasePlayer.LocalPlayer.EyePos, hitPosition);
            var distance2 = Vector3.Distance(BasePlayer.LocalPlayer.Position, attack.playerAttack.attack.hitPositionWorld);
//                ConsoleSystem.Log("Distance2 => " +distance2);
//                ConsoleSystem.Log("Distance => " +GetTimeout(FiredProjectiles[attack.playerAttack.projectileID], distance2));
            float timeout = 0;
            if (distance2 < distance)
                timeout = GetTimeout(FiredProjectiles[attack.playerAttack.projectileID], distance - distance2);
            if (timeout <= 0) timeout = 0.001f;
            var player = this.TargetPlayer;
            var attackCopy = attack.Copy();

            var start = TargetPlayer.Position + (BasePlayer.LocalPlayer.Position - TargetPlayer.Position + new Vector3(0, TargetPlayer.GetHeight()));
            var finish = TargetPlayer.Position + new Vector3(0, TargetPlayer.GetHeight());
            SapphireEngine.Functions.Timer.SetTimeout(() =>
            {
                ConsoleSystem.LogWarning("[Silent] Sleep => " + timeout + this.TargetPlayer.ModelState.flags);
                SendRangeAttack(player, attackCopy, finish, start);
            }, timeout);
            return true;
        }

        return false;
    }

    public bool SendRangeAttack(BasePlayer target, PlayerProjectileAttack parentAttack, Vector3 pointEnd, Vector3 pointStart)
    {
        if (target.IsAlive)
        {
            parentAttack.hitDistance = Vector3.Distance(target.Position, BasePlayer.LocalPlayer.Position);
            parentAttack.playerAttack.attack.hitBone = 698017942;
            parentAttack.playerAttack.attack.hitPartID = 0;
            // parentAttack.playerAttack.attack.hitNormalLocal = hitInfo.HitNormalPos;
            // parentAttack.playerAttack.attack.hitPositionLocal = hitInfo.HitLocalPos;
            parentAttack.playerAttack.attack.hitID = target.UID;

            float height = target.GetHeight();
            DDraw.Arrow(pointStart, pointEnd, 0.1f, Color.blue, 5f);
            parentAttack.playerAttack.attack.hitPositionWorld = pointEnd;
            // parentAttack.playerAttack.attack.hitNormalWorld = BasePlayer.LocalPlayer.GetForward();
            // parentAttack.playerAttack.attack.hitPositionLocal = new Vector3(-0.1f, 0, 0.1f);
            parentAttack.playerAttack.attack.pointEnd = parentAttack.playerAttack.attack.hitPositionWorld;
            parentAttack.playerAttack.attack.pointStart = pointStart;

//                var forward = GetForward();
//                parentAttack.playerAttack.attack.hitPositionWorld = EyePos + GetForward();
//                parentAttack.playerAttack.attack.hitNormalWorld = EyePos + GetForward();
//                parentAttack.playerAttack.attack.pointEnd = EyePos + GetForward();

            ConsoleSystem.Log("attack.playerAttack.attack.hitID: " + parentAttack.playerAttack.attack.hitID);
            ConsoleSystem.Log("attack.playerAttack.attack.hitBone: " + parentAttack.playerAttack.attack.hitBone);
            ConsoleSystem.Log("attack.playerAttack.attack.pointStart: " + parentAttack.playerAttack.attack.pointStart + ", LocalPlayerPos: " + BasePlayer.LocalPlayer.Position);
            ConsoleSystem.Log("attack.playerAttack.attack.pointEnd: " + parentAttack.playerAttack.attack.pointEnd + ", TargetPlayerPos: " + TargetPlayer?.Position);
            ConsoleSystem.Log("attack.playerAttack.attack.hitPositionLocal: " + parentAttack.playerAttack.attack.hitPositionLocal);
            ConsoleSystem.Log("attack.playerAttack.attack.hitPositionWorld: " + parentAttack.playerAttack.attack.hitPositionWorld);
            ConsoleSystem.Log("attack.playerAttack.attack.hitNormalLocal: " + parentAttack.playerAttack.attack.hitNormalLocal);
            ConsoleSystem.Log("attack.playerAttack.attack.hitNormalWorld: " + parentAttack.playerAttack.attack.hitNormalWorld + "#########");


            VirtualServer.BaseClient.write.Start();
            VirtualServer.BaseClient.write.PacketID(Message.Type.RPCMessage);
            VirtualServer.BaseClient.write.UInt32(BasePlayer.LocalPlayer.UID);
            VirtualServer.BaseClient.write.UInt32((uint) ERPCMethodUID.OnProjectileAttack);
            PlayerProjectileAttack.Serialize(VirtualServer.BaseClient.write, parentAttack);
            VirtualServer.BaseClient.write.Send(new SendInfo(VirtualServer.BaseClient.Connection));
        }

        return true;
    }

    public void Unloaded()
    {
        this.HasPluginUnloaded = true;
    }

    public void OnPlayerTick(PlayerTick playerTick, PlayerTick previousTick)
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

    private bool RPC_OnCLProject(Message message)
    {
        using (ProjectileShoot projectileShoot = ProjectileShoot.Deserialize(message.read))
        {
            foreach (ProjectileShoot.Projectile projectile in projectileShoot.projectiles)
            {
                RangeAim.NoteFiredProjectile(projectile.projectileID, BasePlayer.LocalPlayer.ActiveItem.PrefabID, BasePlayer.LocalPlayer.ActiveItem.AmmoType);
            }
        }

        return false;
    }

    private bool OnRpcMessage(Message message)
    {
        message.read._position = 1;
        UInt32 uid = message.read.EntityID();
        ERPCMethodUID rpcMethod = (ERPCMethodUID) message.read.UInt32();


        if (rpcMethod == ERPCMethodUID.OnProjectileAttack)
        {
            return this.RPC_OnProjectileAttack(message);
        }

        if (rpcMethod == ERPCMethodUID.CLProject)
        {
            return this.RPC_OnCLProject(message);
        }

        return false;
    }

    private bool RPC_OnProjectileAttack(Message message)
    {
        using (PlayerProjectileAttack attack = PlayerProjectileAttack.Deserialize(message.read))
        {
            UInt32 hitId = attack.playerAttack.attack.hitID;
            UInt32 hitBone = attack.playerAttack.attack.hitBone;
            var hitPlayer = BaseNetworkable.Get<BasePlayer>(hitId);
            DDraw.Arrow(attack.playerAttack.attack.pointStart, attack.playerAttack.attack.pointEnd, 0.1f, Color.black, 5f);
            ConsoleSystem.Log("########\n\n attack.playerAttack.attack.hitID: " + attack.playerAttack.attack.hitID);
            ConsoleSystem.Log("attack.playerAttack.attack.hitBone: " + attack.playerAttack.attack.hitBone);
            ConsoleSystem.Log("attack.playerAttack.attack.pointStart: " + attack.playerAttack.attack.pointStart + ", LocalPlayerPos: " + BasePlayer.LocalPlayer.Position);
            ConsoleSystem.Log("attack.playerAttack.attack.pointEnd: " + attack.playerAttack.attack.pointEnd + ", TargetPlayerPos: " + TargetPlayer?.Position);
            ConsoleSystem.Log("attack.playerAttack.attack.hitPositionLocal: " + attack.playerAttack.attack.hitPositionLocal);
            ConsoleSystem.Log("attack.playerAttack.attack.hitPositionWorld: " + attack.playerAttack.attack.hitPositionWorld);
            ConsoleSystem.Log("attack.playerAttack.attack.hitNormalLocal: " + attack.playerAttack.attack.hitNormalLocal);
            ConsoleSystem.Log("attack.playerAttack.attack.hitNormalWorld: " + attack.playerAttack.attack.hitNormalWorld + "\n\n--------");

            if (hitId == 0)
            {
                return this.Silent(attack);
            }

            if (hitPlayer == null && hitId != 0 && this.TargetPlayer != null)
            {
                return this.Silent(attack);
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