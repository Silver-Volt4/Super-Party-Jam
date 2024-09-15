using Godot;
using SuperPartyJam.Server;
using SuperPartyJam.Server.Utils;

namespace SuperPartyJam.Game.Dice;

public partial class DiceModule : SPJModule
{
    public override string GetName() => "dice";
    public override SPJModuleMetadata GetMetadata() => new()
    {
        Id = GetName(),
        DisplayName = "Test module",
        Description = "This is not an actual game, it's intended for testing!",
        MinPlayers = 0,
        MaxPlayers = -1,
        Thumbnail = GD.Load<Texture2D>("res://games/dice/gallery/thumbnail.png")
    };

    [Sync(name: "dice")] public SPJState<int> dice = new(0);
}