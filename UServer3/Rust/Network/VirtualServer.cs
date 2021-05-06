using System;
using System.IO;
using ProtoBuf;
using SapphireEngine;
using SapphireEngine.Functions;
using Facepunch.Network;
using Network;
using UnityEngine.AI;
using UServer3.Environments;
using UServer3.Environments.Cryptography;
using UServer3.Rust;
using UServer3.Rust.Rust;
using UServer3.Rust.Struct;
using UServer3.Rust.Struct;
using Client = Facepunch.Network.Raknet.Client;
using Server = Facepunch.Network.Raknet.Server;

namespace UServer3.Rust.Network
{
    public class VirtualServer : SapphireType
    {
        public static VirtualServer Instance { get; private set; }

        public static Server BaseServer;
        public static Client BaseClient;

        public static UserInformation ConnectionInformation { get; private set; }
        public static bool IsHaveConnection => ConnectionInformation != null;
        
        public static UInt32 LastEntityNUM { get; private set; } = 0;
        public static UInt32 TakeEntityNUM => ++LastEntityNUM;
        
        public override void OnAwake()
        {
            Instance = this;
            this.InitializationNetwork();
            ConsoleSystem.Log("[VirtualServer]: Все службы готовы к работе!");
        }

        public override void OnUpdate()
        {
            try
            {
                this.CycleNetwork();
            }
            catch (Exception ex)
            {
                ConsoleSystem.LogError(ex.ToString());
            }
        }

        #region [Method] [Example] CycleNetwork

        private void CycleNetwork()
        {
            BaseClient.Cycle();
            BaseServer.Cycle();
        }

        #endregion

        #region [Class] NetServerHandler
        class NetServerHandler : IServerCallback
        {
            public void OnNetworkMessage(Message message) => Instance.OUT_OnNetworkMessage(message);
            public void OnDisconnected(string reason, Connection connection) => Instance.OUT_OnDisconnected(reason, connection);

            public bool OnUnconnectedMessage(int type, NetRead read, uint ip, int port)
            {
                return false;
            }
        }
        #endregion

        #region [Class] NetClientHandler

        class NetClientHandler : IClientCallback
        {
            public void OnNetworkMessage(Message message) => Instance.IN_OnNetworkMessage(message);
            public void OnClientDisconnected(string reason) => Instance.IN_OnDisconnected(reason);
        }

        #endregion
        
        #region [Method] [Example] InitializationNetwork

        private void InitializationNetwork()
        {
            try
            {
                ConsoleSystem.Log("[VirtualServer]: Служба Network запускается...");
                BaseServer = new Server
                {
                    ip = "127.0.0.1",
                    port = 28015,
                    callbackHandler = new NetServerHandler(),
                };

                BaseClient = new Client()
                {
                    callbackHandler = new NetClientHandler()
                };
                
                BaseServer.cryptography = new NetworkCryptographyServer();
                BaseClient.cryptography = new NetworkCryptographyServer();

                BaseServer.Start();
                ConsoleSystem.Log("[VirtualServer]: Служба Network успешно запущена!");
            }
            catch (Exception ex)
            {
                ConsoleSystem.LogError("[VirtualServer]: Исключение в InitializationNetwork(): " + ex.Message);
            }
        }

        #endregion

        #region [Method] [Example] OnNewConnection

        public void OnNewConnection(Connection connection)
        {
            try
            {
                if (BaseServer.connections.Count <= 1)
                {
                    ConsoleSystem.Log($"[VirtualServer]: Есть новое подключение [{BaseServer.connections[0].ipaddress}]");
                    ConsoleSystem.Log($"[VirtualServer]: Подключаемся к игровому серверу [{Settings.FileSettings.ConnectionIP}:{Settings.FileSettings.ConnectionPort}]");
                    if (BaseClient.Connect(Settings.FileSettings.ConnectionIP, Settings.FileSettings.ConnectionPort))
                    {
                        ConsoleSystem.Log("[VirtualServer]: Инициализация подключения успешно завершена!");
                    }
                    else
                        ConsoleSystem.LogError($"[VirtualServer]: В попытке подключения отказано!");
                }
                else
                    ConsoleSystem.LogError($"[VirtualServer]: Уже есть одно подключение, больше подключений не может быть!");
            }
            catch (Exception ex)
            {
                ConsoleSystem.LogError("[VirtualServer]: Исключение в OnNewConnection(): " + ex.Message);
            }
        }

        #endregion

        #region [Method] [Example] OUT_OnDisconnected

        public void OUT_OnDisconnected(string reason, Connection conn)
        {
            if (BaseClient?.IsConnected() == true)
            {
                ConsoleSystem.LogWarning("[VirtualServer]: Соеденение с игровым клиентом разорвано: " + reason);
                BaseClient?.Disconnect(reason, false);
                if (BaseClient != null & BaseClient.Connection != null)
                {
                    BaseClient.Connection.decryptIncoming = false;
                    BaseClient.Connection.encryptOutgoing = false;
                }
                NetworkManager.Instance.OnDisconnected();
                
                ConnectionInformation = null;
                LastEntityNUM = 0;
            }
        }

        #endregion

        #region [Method] [Example] IN_OnDisconnected

        public void IN_OnDisconnected(string reason)
        {
            if (BaseServer != null && BaseServer.connections.Count > 0)
            {
                BaseServer?.Kick(BaseServer.connections[0], reason, false);
                ConsoleSystem.LogWarning("[VirtualServer]: Соеденение с игровым сервером разорвано: " + reason);
            }
        }

        #endregion

        #region [Method] [Example] OUT_OnNetworkMessage

        public void OUT_OnNetworkMessage(Message packet)
        {
            switch (packet.type)
            {
                case Message.Type.Ready:
                    packet.connection.decryptIncoming = true;
                    SendPacket(BaseClient, packet);
                    BaseClient.Connection.encryptOutgoing = true;
                    return;
                case Message.Type.GiveUserInformation:
                    ConnectionInformation = UserInformation.ParsePacket(packet);
                    this.OnNewConnection(packet.connection);
                    break;
                default:
                    if (NetworkManager.Instance.Out_NetworkMessage(packet) == false)
                        SendPacket(BaseClient, packet);
                    break;
            }
        }

        #endregion

        #region [Method] [Example] IN_OnNetworkMessage

        public void IN_OnNetworkMessage(Message packet)
        {
            switch (packet.type)
            {
                case Message.Type.Approved:
                    OnApproved(packet);
                    break;
                case Message.Type.DisconnectReason:
                    SendPacket(BaseServer, packet);
                    if (BaseServer != null && BaseServer.connections.Count > 0)
                    {
                        packet.read.Position = 1L;
                        string reasone = packet.read.String();
                        BaseServer?.Kick(BaseServer.connections[0], reasone, false);
                        ConsoleSystem.LogWarning("[VirtualServer]: От игрового сервера получена причина дисконнекта: " + reasone);
                    }
                    break;
                case Message.Type.RequestUserInformation:
                    if (BaseClient.write.Start())
                    {
                        BaseClient.write.PacketID(Message.Type.GiveUserInformation);
                        if (Settings.FileSettings.FakeSteamID == 0)
                        {
                            ConnectionInformation.Write(BaseClient);
                        }
                        else
                        {
                            ConnectionInformation.WriteFake(BaseClient, Settings.FileSettings.FakeSteamID, Settings.FileSettings.FakeUsername);
                        }

                        BaseClient.write.Send(new SendInfo());
                    }
                    GameWer.AuthSteamID(ConnectionInformation.SteamIDFromServer);
                    break;
                case Message.Type.Entities:
                    packet.read.UInt32();
                    using (Entity entity = Entity.Deserialize(packet.read))
                    {
                        if (EntityManager.OnEntity(entity) == false)
                        {
                            if (BaseServer.write.Start())
                            {
                                BaseServer.write.PacketID(Message.Type.Entities);
                                BaseServer.write.UInt32(TakeEntityNUM);
                                entity.WriteToStream(BaseServer.write);
                                BaseServer.write.Send(new SendInfo(BaseServer.connections[0]));
                            }
                        }
                    }
                    break;
                case Message.Type.EntityDestroy:
                    EntityManager.OnEntityDestroy(packet);
                    SendPacket(BaseServer, packet);
                    break;
                case Message.Type.EntityPosition:
                    EntityManager.OnEntityPosition(packet);
                    SendPacket(BaseServer, packet);
                    break;
                default:
                    if (NetworkManager.Instance.IN_NetworkMessage(packet) == false)
                        SendPacket(BaseServer, packet);
                    break;
            }
        }

        #endregion

        #region [Method] [Example] OnApproved

        private void OnApproved(Message packet)
        {
            try
            {
                using (Approval approval = Approval.Deserialize(packet.read))
                {
                    ConsoleSystem.LogWarning($"[VirtualServer]: Вы подключились к: {(approval.official ? "[Oficial] " : "")}" + approval.hostname);

                    BaseClient.Connection.encryptionLevel = approval.encryption;
                    BaseClient.Connection.decryptIncoming = true;

                    if (BaseServer.write.Start())
                    {
                        BaseServer.write.PacketID(Message.Type.Approved);
                        approval.steamid = ConnectionInformation.SteamID;
                        Approval.Serialize(BaseServer.write, approval);
                        BaseServer.write.Send(new SendInfo(BaseServer.connections[0]));
                    }
                    
                    for (int i = 0; i < BaseServer.connections.Count; i++)
                    {
                        BaseServer.connections[i].encryptionLevel = 1;
                        BaseServer.connections[i].encryptOutgoing = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleSystem.LogError("[VirtualServer]: Исключение в OnApproved(): " + ex.Message);
            }
        }

        #endregion


        #region [Method] [Example] SendPacket

        public byte[] GetPacketBytes(Message message)
        {
            byte[] buffer = null;
            long start_pos = message.read.Position;
            message.peer.read.Position = 0L;
            using (BinaryReader br = new BinaryReader(message.peer.read))
            {
                buffer = br.ReadBytes((int) message.peer.read.Length);
            }
            message.read.Position = start_pos;
            return buffer;
        }

        public void SendPacket(global::Network.BaseNetwork peer, Message message)
        {
            message.peer.read.Position = 0L;
            using (BinaryReader br = new BinaryReader(message.peer.read))
            {
                peer.write.Start();
                peer.write.Write(br.ReadBytes((int) message.peer.read.Length), 0, (int) message.peer.read.Length);
                peer.write.Send(new SendInfo(peer is Client ? BaseClient.Connection : BaseServer.connections[0]));
            }
        }

        public void SendPacket(global::Network.BaseNetwork peer, byte[] message)
        {
            peer.write.Start();
            peer.write.Write(message, 0, (int) message.Length);
            peer.write.Send(new SendInfo(peer is Client ? BaseClient.Connection : BaseServer.connections[0]));
        }

        #endregion
    }
}