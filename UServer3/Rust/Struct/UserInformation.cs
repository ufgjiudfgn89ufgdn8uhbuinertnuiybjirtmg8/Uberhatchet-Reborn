using System;
using System.IO;
using System.Net;
using Network;
using Rust;
using SapphireEngine;

namespace UServer3.Rust.Struct
{
    public class UserInformation
    {
        public Byte PacketProtocol;
        public UInt64 SteamID;
        public UInt32 ConnectionProtocol;
        public String OS;
        public String Username;
        public String Branch;
        public Byte[] SteamToken;

        public static UserInformation ParsePacket(Message message)
        {
            try
            {
                message.read.Position = 1;
                var userInformation = new UserInformation();
                userInformation.PacketProtocol = message.read.UInt8();
                userInformation.SteamID = message.read.UInt64();
                userInformation.ConnectionProtocol = message.read.UInt32();
                userInformation.OS = message.read.String();
                userInformation.Username = message.read.String();
                userInformation.Branch = message.read.String();
                userInformation.SteamToken = message.read.BytesWithSize();
                
                return userInformation;
            }
            catch (Exception ex)
            {
                ConsoleSystem.LogError("Error to Struct.UserInformation.ParsePacket(): " + ex.Message);
            }
            
            
            return default(UserInformation);
        }

        public void Write(global::Network.BaseNetwork peer)
        {
            peer.write.UInt8(PacketProtocol);
            peer.write.UInt64(SteamID);
            peer.write.UInt32(ConnectionProtocol);
            peer.write.String(OS);
            peer.write.String(Username);
            peer.write.String(Branch);
            peer.write.BytesWithSize(SteamToken);
        }

        public void WriteFake(global::Network.BaseNetwork peer, ulong fakeSteamID, string fakeUsername)
        {
            peer.write.UInt8(PacketProtocol);
            peer.write.UInt64(fakeSteamID);
            peer.write.UInt32(ConnectionProtocol);
            peer.write.String(OS);
            peer.write.String(fakeUsername);
            peer.write.String(Branch);
            peer.write.BytesWithSize(this.TakeFakeSteamToken(fakeSteamID));
        }

        private byte[] TakeFakeSteamToken(ulong fakeSteamID)
        {
            byte[] newToken = new byte[SteamToken.Length];
            SteamToken.CopyTo(newToken, 0);

            byte[] realSteamIDBuffer = BitConverter.GetBytes(SteamID);
            byte[] fakeSteamIDBuffer = BitConverter.GetBytes(fakeSteamID);
            for (var i = 0; i < newToken.Length; i++)
            {
                if (i + 7 < newToken.Length)
                {
                    try
                    {
                        if (newToken[i] == realSteamIDBuffer[0] && newToken[i + 1] == realSteamIDBuffer[1] && newToken[i + 2] == realSteamIDBuffer[2] && newToken[i + 3] == realSteamIDBuffer[3] && newToken[i + 4] == realSteamIDBuffer[4] && newToken[i + 5] == realSteamIDBuffer[5] && newToken[i + 6] == realSteamIDBuffer[6] && newToken[i + 7] == realSteamIDBuffer[7])
                        {
                            newToken[i] = fakeSteamIDBuffer[0];
                            newToken[i + 1] = fakeSteamIDBuffer[1];
                            newToken[i + 2] = fakeSteamIDBuffer[2];
                            newToken[i + 3] = fakeSteamIDBuffer[3];
                            newToken[i + 4] = fakeSteamIDBuffer[4];
                            newToken[i + 5] = fakeSteamIDBuffer[5];
                            newToken[i + 6] = fakeSteamIDBuffer[6];
                            newToken[i + 7] = fakeSteamIDBuffer[7];
                        }
                    }
                    catch
                    {
                        
                    }
                }
            }

            return newToken;
        }
    }
}