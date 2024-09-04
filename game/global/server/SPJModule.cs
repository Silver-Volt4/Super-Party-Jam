using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

public abstract partial class SPJModule : Resource, SPJEventHook
{
    protected SPJPlayer player;
    private SPJStorage _storage = new SPJStorage();

    public void SetPlayer(SPJPlayer player)
    {
        this.player = player;
        this.SetupEventHook();
    }

    public SPJClient GetClient()
    {
        return player.GetClient();
    }

    public SPJPacket.PacketPhase GetPhase()
    {
        return SPJPacket.PacketPhase.Module;
    }

    public SPJStorage GetStorage()
    {
        return _storage;
    }
}
