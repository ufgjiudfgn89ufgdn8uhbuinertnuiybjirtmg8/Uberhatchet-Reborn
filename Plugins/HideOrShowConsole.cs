using System;
using System.Runtime.InteropServices;
using Network;
using ProtoBuf;
using UnityEngine;
using UServer3.Rust.Struct;

public class HideOrShowConsole : IPlugin
{
    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    public void Loaded()
    {
        var handle = GetConsoleWindow();

        // Hide
        ShowWindow(handle, SW_HIDE);

        //// Show
        //ShowWindow(handle, SW_SHOW);
    }

    public void Unloaded()
    {
    }

    public void OnPlayerTick(PlayerTick playerTick, PlayerTick previousTick)
    {
    }

    public bool Out_NetworkMessage(Message message)
    {
        return false;
    }

    public bool In_NetworkMessage(Message message)
    {
        return false;
    }

    public void OnPacketEntityCreate(Entity entityPacket)
    {
        return;
    }

    public bool OnPacketEntityUpdate(Entity entityPacket)
    {
        return false;
    }

    public void OnPacketEntityPosition(uint uid, Vector3 position, Vector3 rotation)
    {
        return;
    }

    public void OnPacketEntityDestroy(uint uid)
    {
        return;
    }

    public bool CallHook(string name, object[] args)
    {
        return false;
    }
}