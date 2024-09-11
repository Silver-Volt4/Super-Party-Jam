using Godot;
using System;
using System.Collections.Generic;

public partial class DiceGame : Node2D
{
	private SPJPlayer[] players;

	public override void _Ready()
	{
		players = SPJ.Meta.GetPlayers();
		var profileScene = GD.Load<PackedScene>("res://games/dice/DiceProfile.tscn");
		var idx = 0;
		foreach (var player in players)
		{
			var profile = profileScene.Instantiate<DiceProfile>();
			profile.module = player.GetModule() as DiceModule;
			AddChild(profile);
			profile.Position = new Vector2(20 * idx, 40);
			idx += 1;
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
