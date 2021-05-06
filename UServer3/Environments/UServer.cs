using System;
using System.IO;
using SapphireEngine;
using UServer3.CSharp.Reflection;
using UServer3.Rust.Data;
using UServer3.Rust.Functions;
using UServer3.Rust.Network;
using UServer3.Rust.Functions;

namespace UServer3.Environments
{
    public class UServer : SapphireType
    {
        public static void Initialization() => Framework.Initialization<UServer>();
        
        public override void OnAwake()
        {
            ConsoleSystem.OutputPath = Bootstrap.OutputPath;
            ConsoleSystem.Log("[Bootstrap]: Приложение запущено");

            Settings.Init();
            DatabaseLoader.Load<Database>();
            RPCManager.Initialize();
            this.AddType<VirtualServer>();
            this.AddType<NetworkManager>();
            
            this.AddType<WallHack>();
        }
    }
}