using Godot;

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
}