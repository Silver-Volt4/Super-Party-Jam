using Godot;
using System;

public partial class MainMenu : TextureRect
{
	[Export] protected BeginScreen beginScreen;
	[Export] protected Lobby lobby;

	public override void _Ready()
	{
		beginScreen.PlayLocal += OnPlayLocal;
		beginScreen.PlayRemote += OnPlayRemote;
	}

	private void OnPlayLocal()
	{
		Start();
	}

	private void OnPlayRemote()
	{
		SPJ.Meta.Alert("Not implemented", "This feature is not available yet.");
	}

	public void Start()
	{
		SPJ.GameServer.Start();
		SPJ.HttpServer.Start();
		var tw = CreateTween();
		tw.TweenProperty(beginScreen, "modulate:a", 0, 0.1);
		tw.TweenCallback(new Callable(beginScreen, BeginScreen.MethodName.QueueFree));
		tw.TweenCallback(Callable.From(() =>
		{
			lobby.Modulate = lobby.Modulate with { A = 0f };
			lobby.Show();
			lobby.Setup();
		}));
		tw.TweenProperty(lobby, "modulate:a", 1.0, 0.1);
	}
}
