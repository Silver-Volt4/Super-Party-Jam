using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;
using SuperPartyJam.Server.Utils;

namespace SuperPartyJam.Server;

public abstract partial class SPJModule : Resource, MPacketHandler
{
    protected SPJPlayer player;

    public new abstract string GetName();
    public abstract SPJModuleMetadata GetMetadata();

    public enum CloseReason
    {
        SwitchToModule = 4300,
        ClearModule = 4301,
    }

    public void SetPlayer(SPJPlayer player)
    {
        this.player = player;
        this.InitHandler();
        player.UnhandledPacket += this.HandlePacket;
    }

    public SPJClient GetClient()
    {
        return player.GetClient();
    }

    public PacketPhase GetPhase()
    {
        return PacketPhase.Module;
    }

    internal void ChangeModule(string v)
    {
        throw new NotImplementedException();
    }
}

public class SPJModuleMetadata
{
    public string Id;
    public string DisplayName;
    public string Description;
    public int MinPlayers;
    public int MaxPlayers;
    public Texture2D Thumbnail;

    public string DescribePlayerCount()
    {
        if (MinPlayers == -1 && MaxPlayers == -1)
        {
            return "Any player count";
        }

        if (MinPlayers == -1)
        {
            return $"<{MaxPlayers} players";
        }

        if (MaxPlayers == -1)
        {
            return $"{MinPlayers}+ players";
        }

        return $"{MinPlayers}-{MaxPlayers} players";
    }

    public bool PlayerCountSatisfied(int players)
    {
        if (MinPlayers == -1 && MaxPlayers == -1)
        {
            return players >= 1;
        }

        if (MinPlayers == -1)
        {
            return players <= MaxPlayers;
        }

        if (MaxPlayers == -1)
        {
            return players >= MinPlayers;
        }

        return players <= MaxPlayers && players >= MinPlayers;
    }
}