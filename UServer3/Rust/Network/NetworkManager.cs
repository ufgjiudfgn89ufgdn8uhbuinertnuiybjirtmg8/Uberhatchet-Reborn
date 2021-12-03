using System;
using Network;
using SapphireEngine;
using UServer3.CSharp.Reflection;
using UServer3.Environments;
using UServer3.Rust;
using UServer3.Rust.Data;
using UServer3.Rust.Struct;

namespace UServer3.Rust.Network
{
    public class NetworkManager : SapphireType
    {
        public static NetworkManager Instance;
        
        public override void OnAwake() => Instance = this;

        public bool IN_NetworkMessage(Message message)
        {
            bool returnResult = false;
            switch (message.type)
            {
                case Message.Type.RPCMessage:
                    bool resultRPC = OnRPCMessage(ERPCNetworkType.IN, message);
                    if (resultRPC == true)
                    {
                        returnResult = resultRPC;
                    }
                    break;
            }

            bool resultHook = PluginManager.Instance.CallHook_In_NetworkMessage(message);
            if (resultHook == true)
            {
                returnResult = resultHook;
            }
            
            return returnResult;
        }

        public bool Out_NetworkMessage(Message message)
        {
            bool returnResult = false;
            
            switch (message.type)
            {
                case Message.Type.Tick:
                    bool resultT = false;
                    if (BasePlayer.IsHaveLocalPlayer)
                    {
                        resultT = BasePlayer.LocalPlayer.OnTick(message);
                    }
                    if (resultT == true)
                    {
                        returnResult = resultT;
                    }
                    break;
                case Message.Type.RPCMessage:
                    bool resultR = OnRPCMessage(ERPCNetworkType.OUT, message);
                    if (resultR == true)
                    {
                        returnResult = resultR;
                    }
                    break;
            }

            bool resultHook = PluginManager.Instance.CallHook_Out_NetworkMessage(message);
            if (resultHook == true)
            {
                returnResult = resultHook;
            }
            
            return returnResult;
        }

        private static bool OnRPCMessage(ERPCNetworkType type, Message message)
        {
            UInt32 UID = message.read.EntityID();
            UInt32 rpcId = message.read.UInt32();
            if (type == ERPCNetworkType.IN)
                message.read.UInt64();
            
            return RPCManager.RunRPCMethod(UID, (ERPCMethodUID) rpcId, type, message);
        }

        public void OnDisconnected()
        {
            BaseNetworkable.DestroyAll();
        }
    }
}