using Godot;

partial class DiceModule : SPJModule
{
    public override SPJModuleMetadata GetMetadata() => new()
    {
        Id = "dice",
        DisplayName = "Test module",
        Description = "This is not an actual game, it's intended for testing!",
        MinPlayers = 0,
        MaxPlayers = -1,
        Thumbnail = GD.Load<Texture2D>("res://games/dice/gallery/thumbnail.png")
    };
}