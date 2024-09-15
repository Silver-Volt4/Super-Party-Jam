using System;
using System.Linq;
using Godot;
using SuperPartyJam.Server;

namespace SuperPartyJam.Scenes;

public partial class SPJMeta : Node
{
	[Export] private CanvasLayer overlays;

	private SPJPlayer[] players = { };

	public override void _Ready()
	{
		base._Ready();
		SPJ.Meta = this;
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event.IsActionPressed("fullscreen"))
		{
			GetWindow().Mode = GetWindow().Mode == Window.ModeEnum.ExclusiveFullscreen ? Window.ModeEnum.Windowed : Window.ModeEnum.ExclusiveFullscreen;
		}
	}

	public void PlaySfx(string sound)
	{
		GetNode<AudioStreamPlayer>(sound).Play();
	}

	public void LaunchGame(string name)
	{
		var scene = GD.Load<PackedScene>($"res://games/{name}/root.tscn");
		var activePlayers = SPJ.GameServer.GetPlayers().Where(p => !p.IsSpectator());
		var module = SPJ.LoadModuleClass(name);
		foreach (var player in activePlayers)
		{
			player.SetModule(module.New().As<SPJModule>());
		}
		players = activePlayers.ToArray();
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToPacked, scene);
	}

	public void Alert(string title, string text)
	{
		var dialog = GD.Load<PackedScene>("res://scenes/dialog/Dialog.tscn").Instantiate<Dialog>();
		overlays.AddChild(dialog.Title(title).Text(text).Alert("OK"));
	}

	public SPJPlayer[] GetPlayers() => players;
}