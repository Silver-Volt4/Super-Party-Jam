using Godot;
using System;

public partial class DiceProfile : Label
{
	public DiceModule module;

	public override void _Ready()
	{
		module.dice.Change += DiceChange;
	}

	private void DiceChange(int new_value)
	{
		GD.Print("state change ", new_value);
		Text = new_value.ToString();
	}
}
