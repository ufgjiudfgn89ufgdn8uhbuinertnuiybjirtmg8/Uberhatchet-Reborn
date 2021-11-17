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
                catch(Exception ex)
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
                ConsoleSystem.LogError($"Exception from [{name}] plugin, this plugin is not loaded: " + e);
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
                catch(Exception ex)
                {
                    ConsoleSystem.LogError($"Exception from [{name}] plugin in Unloaded method: " + ex);
                }
                ConsoleSystem.Log($"Plugin [{name}] has been Unloaded!");
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
                    bool result = false;
                    switch (name)
                    {
                        case "Out_NetworkMessage":
                            result = pluginKeyPair.Value.Out_NetworkMessage(args[0] as Message);
                            break;
                        case "In_NetworkMessage":
                            result = pluginKeyPair.Value.In_NetworkMessage(args[0] as Message);
                            break;
                        case "OnPacketEntityCreate":
                            pluginKeyPair.Value.OnPacketEntityCreate(args[0] as Entity);
                            break;
                        case "OnPacketEntityUpdate":
                            result = pluginKeyPair.Value.OnPacketEntityUpdate(args[0] as Entity);
                            break;
                        case "OnPacketEntityPosition":
                            pluginKeyPair.Value.OnPacketEntityPosition((uint)args[0], (Vector3)args[1], (Vector3)args[2]);
                            break;
                        case "OnPacketEntityDestroy":
                            pluginKeyPair.Value.OnPacketEntityDestroy((uint)args[0]);
                            break;
                        case "OnPlayerTick":
                            pluginKeyPair.Value.OnPlayerTick((PlayerTick)args[0], (PlayerTick)args[1]);
                            break;
                        default:
                            result = pluginKeyPair.Value.CallHook(name, args);
                            break;
                    }
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