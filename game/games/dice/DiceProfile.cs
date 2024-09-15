using Godot;

namespace SuperPartyJam.Game.Dice;

public partial class DiceProfile : Label
{
	public DiceModule module;

	public override void _Ready()
	{
		module.dice.Change += DiceChange;
	}

	private void DiceChange(int new_value)
	{
		Text = $"{module.GetPlayer().username.Get()}: {new_value}";
	}
}
