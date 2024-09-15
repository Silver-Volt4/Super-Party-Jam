using System;
using Godot;
using Godot.Collections;
using SuperPartyJam;
using SuperPartyJam.Server;
using SuperPartyJam.Server.Utils;

partial class BingoModule : SPJModule
{
    public override string GetName() => "bingo";
    public override SPJModuleMetadata GetMetadata() => new()
    {
        Id = "bingo",
        DisplayName = "Sonic Bingo Party",
        Description = "Enjoy a fun bingo session with your friends. Unlimited players!",
        MinPlayers = 2,
        MaxPlayers = -1,
        Thumbnail = GD.Load<Texture2D>("res://games/bingo/gallery/thumbnail.png")
    };

    private static int[]? _bingonumbers = null;

    int[] GetRandomBingoCard()
    {
        if (_bingonumbers == null)
        {
            int[] ints = new int[75];
            for (int i = 1; i <= 75; i++)
            {
                ints[i - 1] = i;
            }
            _bingonumbers = ints;
        }
        SPJ.Shuffle(_bingonumbers);
        return _bingonumbers[..25];
    }

    BingoModule() : base()
    {

    }

    [Sync("bingo_card", true)] private SPJState<int[]> bingo_card = new(new int[25]);
}