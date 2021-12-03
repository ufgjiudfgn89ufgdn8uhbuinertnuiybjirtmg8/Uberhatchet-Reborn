using System;
using System.Collections.Generic;
using System.IO;
using CenterDevice.MiniFSWatcher;
using CSScriptLib;
using Network;
using ProtoBuf;
using SapphireEngine;
using UnityEngine;
using UServer3.Rust.Data;
using UServer3.Rust.Struct;

namespace UServer3.Environments
{
    public class PluginManager : SapphireType
    {
        public static PluginManager Instance { get; private set; }
        public static Dictionary<string, IPlugin> ListLoadedPlugins = new Dictionary<string, IPlugin>();

        public FileSystemWatcher Watcher { get; private set; }

        public override void OnAwake()
        {
            Instance = this;
            if (Directory.Exists(Bootstrap.PluginsPath) == false)
            {
                Directory.CreateDirectory(Bootstrap.PluginsPath);
            }

            this.Watcher = new FileSystemWatcher();
            this.Watcher.Path = Bootstrap.PluginsPath;
            this.Watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                                                 | NotifyFilters.FileName;
            this.Watcher.Filter = "*.cs";
            this.Watcher.Created += new FileSystemEventHandler(this.OnFileCreated);
            this.Watcher.Changed += new FileSystemEventHandler(this.OnFileChanged);
            this.Watcher.Deleted += new FileSystemEventHandler(this.OnFileDeleted);
            this.Watcher.EnableRaisingEvents = true;


            DirectoryInfo di = new DirectoryInfo(Bootstrap.PluginsPath);
            var files = di.GetFiles("*.cs");
            for (var i = 0; i < files.Length; i++)
            {
                this.OnFileCreated(files[i].Name, files[i].FullName);
            }

            ConsoleSystem.Log("[PluginManager]: Start...");
        }

        public void OnFileCreated(object source, FileSystemEventArgs e)
        {
            this.OnFileCreated(e.Name, e.FullPath);
        }

        public void OnFileCreated(string name, string path)
        {
            ConsoleSystem.Log("[PluginManager]: OnFileCreated({0})", name);
            try
            {
                IPlugin plugin = CSScript.Evaluator.LoadCode<IPlugin>(File.ReadAllText(path));
                ListLoadedPlugins[name] = plugin;
                try
                {
                    plugin.Loaded();
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from [{name}] plugin in Loaded method: " + ex);
                }

                ConsoleSystem.Log($"Plugin [{name}] has been Loaded!");
            }
            catch (Exception e)
            {
                ConsoleSystem.LogError($"Exception from [{name}] plugin, this plugin is not loaded: " + e);
            }
        }

        public void OnFileChanged(object source, FileSystemEventArgs e)
        {
            this.OnFileChanged(e.Name, e.FullPath);
        }

        public void OnFileChanged(string name, string path)
        {
            ConsoleSystem.Log("[PluginManager]: OnFileChanged({0})", name);
            if (ListLoadedPlugins.TryGetValue(name, out IPlugin plug))
            {
                ListLoadedPlugins.Remove(name);
                try
                {
                    plug.Unloaded();
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from [{name}] plugin in Unloaded method: " + ex);
                }

                ConsoleSystem.Log($"Plugin [{name}] has been Unloaded!");
            }

            try
            {
                IPlugin plugin = CSScript.Evaluator.LoadCode<IPlugin>(File.ReadAllText(path));

                ListLoadedPlugins[name] = plugin;
                try
                {
                    plugin.Loaded();
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from [{name}] plugin in Loaded method: " + ex);
                }

                ConsoleSystem.Log($"Plugin [{name}] has been Loaded!");
            }
            catch (Exception e)
            {
                try
                {
                    ConsoleSystem.LogError($"Exception from [{name}] plugin, this plugin is not loaded: " + e);
                }
                catch
                {
                }
            }
        }

        public void OnFileDeleted(object source, FileSystemEventArgs e)
        {
            this.OnFileDeleted(e.Name, e.FullPath);
        }

        public void OnFileDeleted(string name, string path)
        {
            ConsoleSystem.Log("[PluginManager]: OnFileDeleted({0})", name);
            if (ListLoadedPlugins.TryGetValue(name, out IPlugin plug))
            {
                ListLoadedPlugins.Remove(name);
                try
                {
                    plug.Unloaded();
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from [{name}] plugin in Unloaded method: " + ex);
                }

                ConsoleSystem.Log($"Plugin [{name}] has been Unloaded!");
            }
        }

        public bool CallHook_Out_NetworkMessage(Message message)
        {
            bool returnResult = false;
            foreach (var pluginKeyPair in ListLoadedPlugins)
            {
                try
                {
                    bool result = pluginKeyPair.Value.Out_NetworkMessage(message);
                    if (result == true)
                    {
                        returnResult = true;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from CallHook(Out_NetworkMessage) in [{pluginKeyPair.Key}] plugin: " + ex);
                }
            }

            return returnResult;
        }

        public bool CallHook_In_NetworkMessage(Message message)
        {
            bool returnResult = false;
            foreach (var pluginKeyPair in ListLoadedPlugins)
            {
                try
                {
                    bool result = pluginKeyPair.Value.In_NetworkMessage(message);
                    if (result == true)
                    {
                        returnResult = true;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from CallHook(In_NetworkMessage) in [{pluginKeyPair.Key}] plugin: " + ex);
                }
            }

            return returnResult;
        }

        public void CallHook_OnPacketEntityCreate(Entity entity)
        {
            foreach (var pluginKeyPair in ListLoadedPlugins)
            {
                try
                {
                    pluginKeyPair.Value.OnPacketEntityCreate(entity);
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from CallHook(OnPacketEntityCreate) in [{pluginKeyPair.Key}] plugin: " + ex);
                }
            }
        }

        public bool CallHook_OnPacketEntityUpdate(Entity entity)
        {
            bool returnResult = false;
            foreach (var pluginKeyPair in ListLoadedPlugins)
            {
                try
                {
                    bool result = pluginKeyPair.Value.OnPacketEntityUpdate(entity);
                    if (result == true)
                    {
                        returnResult = true;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from CallHook(OnPacketEntityUpdate) in [{pluginKeyPair.Key}] plugin: " + ex);
                }
            }

            return returnResult;
        }

        public void CallHook_OnPacketEntityPosition(uint uid, Vector3 position, Vector3 rotation)
        {
            foreach (var pluginKeyPair in ListLoadedPlugins)
            {
                try
                {
                    pluginKeyPair.Value.OnPacketEntityPosition(uid, position, rotation);
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from CallHook(OnPacketEntityPosition) in [{pluginKeyPair.Key}] plugin: " + ex);
                }
            }
        }

        public void CallHook_OnPacketEntityDestroy(uint uid)
        {
            foreach (var pluginKeyPair in ListLoadedPlugins)
            {
                try
                {
                    pluginKeyPair.Value.OnPacketEntityDestroy(uid);
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from CallHook(OnPacketEntityDestroy) in [{pluginKeyPair.Key}] plugin: " + ex);
                }
            }
        }

        public void CallHook_OnPlayerTick(PlayerTick tick, PlayerTick tickDelay)
        {
            foreach (var pluginKeyPair in ListLoadedPlugins)
            {
                try
                {
                    pluginKeyPair.Value.OnPlayerTick(tick, tickDelay);
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from CallHook(Out_NetworkMessage) in [{pluginKeyPair.Key}] plugin: " + ex);
                }
            }
        }

        public bool CallHook(string name, object[] args, bool dropMessagePosition = false)
        {
            bool returnResult = false;
            foreach (var pluginKeyPair in ListLoadedPlugins)
            {
                if (args.Length > 0 && args[0] is Message message && dropMessagePosition == true)
                {
                    message.read._position = 1;
                }

                if (args.Length > 2 && args[2] is Message rpcMessage && dropMessagePosition == true)
                {
                    rpcMessage.read._position = 1;
                }

                try
                {
                    bool result = pluginKeyPair.Value.CallHook(name, args);

                    if (result == true)
                    {
                        returnResult = result;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from CallHook({name}) in [{pluginKeyPair.Key}] plugin: " + ex);
                }
            }

            return returnResult;
        }
    }
}