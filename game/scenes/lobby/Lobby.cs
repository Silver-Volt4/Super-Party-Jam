using Godot;
using SuperPartyJam.Server;

namespace SuperPartyJam.Scenes.Lobby;

public partial class Lobby : Control
{
	[Export] private TextureRect qr_code;
	[Export] private GridContainer games_grid;
	[Export] private Control game_info;
	[Export] private Control players;

	public override void _Ready()
	{
		var games = DirAccess.GetDirectoriesAt("res://games/");
		var buttonBase = GD.Load<PackedScene>("res://scenes/lobby/ModuleButton.tscn").Instantiate<ModuleButton>();
		foreach (var game in SPJ.GetMinigames())
		{
			var button = buttonBase.Duplicate() as ModuleButton;
			button.metadata = game;
			games_grid.AddChild(button);
			button.FocusEntered += () => TouchGame(button);
			button.Pressed += () => PlayGame(button);
		}
		foreach (var player in SPJ.GameServer.GetPlayers())
		{
			AddPlayer(player, true);
		}
		SPJ.GameServer.NewPlayer += AddPlayer;
	}

	private void TouchGame(ModuleButton button)
	{
		game_info.GetNode<Label>("Name").Text = button.metadata.DisplayName;
		game_info.GetNode<Label>("Description").Text = button.metadata.Description;
		game_info.GetNode<Label>("Players").Text = button.metadata.DescribePlayerCount();
	}

	private void PlayGame(ModuleButton button)
	{
		SPJModuleMetadata metadata = button.metadata;
		if (metadata.PlayerCountSatisfied(SPJ.GameServer.GetPlayers().Length))
		{
			LaunchGame(metadata);
		}
		else
		{
			SPJ.Meta.Alert("Insufficient player count",
			 "You do not have enough players to play this game."
			);
		}
	}

	private void LaunchGame(SPJModuleMetadata metadata)
	{
		SPJ.Meta.PlaySfx("select");
		SPJ.Meta.LaunchGame("dice");
	}

	public void AddPlayer(SPJPlayer player) => AddPlayer(player, false);
	public void AddPlayer(SPJPlayer player, bool initial = false)
	{
		var lp = GD.Load<PackedScene>("res://scenes/lobby/LobbyPlayer.tscn").Instantiate<LobbyPlayer>();
		lp.Player = player;
		players.AddChild(lp);
		if (!initial) lp.Flash();
	}

	public void Setup()
	{
		var self_ip = SPJ.GetSelfIp();
		if (self_ip == "")
		{
			SPJ.Meta.Alert(
				"Cannot display QR code",
				"Automatic detection of your IP address has failed.\nThe QR code which allows you to quickly join the game cannot be shown.\nPlease connect manually."
			);
		}
		var port = SPJ.HttpServer.GetPort();
		qr_code.Texture = ImageTexture.CreateFromImage(SPJ.GetQRCode($"http://{self_ip}:{port}"));
	}
}
