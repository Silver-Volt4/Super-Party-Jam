using Godot;
using System;

public partial class Lobby : Control
{
	[Export] private TextureRect qr_code;
	[Export] private GridContainer games_grid;
	[Export] private Control game_info;

	public override void _Ready()
	{
		var games = DirAccess.GetDirectoriesAt("res://games/");
		var buttonBase = GD.Load<PackedScene>("res://global/lobby/modulebutton.tscn").Instantiate<ModuleButton>();
		foreach (var game in games)
		{
			var button = buttonBase.Duplicate() as ModuleButton;
			var module = GD.Load<CSharpScript>($"res://games/{game}/{game.Capitalize()}Module.cs").New().As<SPJModule>();
			button.metadata = module.GetMetadata();
			games_grid.AddChild(button);
			button.FocusEntered += () => TouchGame(button);
			button.Pressed += () => PlayGame(button);
		}
		// for player in GameServer.get_players():
		// self.add_player(player, true)
		// GameServer.new_player.connect(self.add_player)		
	}

	private void TouchGame(ModuleButton button)
	{
		game_info.GetNode<Label>("Name").Text = button.metadata.DisplayName;
		game_info.GetNode<Label>("Description").Text = button.metadata.Description;
		game_info.GetNode<Label>("Players").Text = button.metadata.DescribePlayerCount();
	}

	private void PlayGame(ModuleButton button)
	{
	}

	public void Setup()
	{
		var self_ip = SPJHelpers.GetSelfIp();
		if (self_ip == "")
		{
			SPJ.Meta.Alert(
				"Cannot display QR code",
				"Automatic detection of your IP address has failed.\nThe QR code which allows you to quickly join the game cannot be shown.\nPlease connect manually."
			);
		}
		var port = SPJ.GameServer.GetPort();
		qr_code.Texture = ImageTexture.CreateFromImage(SPJHelpers.GetQRCode($"http://{self_ip}:{port}"));
	}
}
