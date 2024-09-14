using Godot;
using System;

public partial class LobbyPlayer : Panel
{
	[Export] private MenuButton options;
	[Export] private Label nameLabel;
	[Export] private Control backdrop;

	public SPJPlayer Player;

	public override void _Ready()
	{
		options.GetPopup().IdPressed += OptionSelected;
		UpdateButtons();
		UpdateName();
		Player.username.ChangeAction += UpdateName;
		Player.spectator.Change += SpectatorChanged;
		Player.force_spectator.Change += SpectatorChanged;
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		Player.username.ChangeAction -= UpdateName;
		Player.spectator.Change -= SpectatorChanged;
		Player.force_spectator.Change -= SpectatorChanged;
	}

	public void Flash()
	{
		UpdateButtons();
		SpectatorChanged(Player.IsSpectator());
		var tw = GetTree().CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Circ);
		tw.TweenMethod(Godot.Callable.From((float p) => (Material as ShaderMaterial)?.SetShaderParameter("progress", p)), 1.0, 0.115, 0.3);
		tw.Parallel().TweenProperty(nameLabel, "position", nameLabel.Position, 0.5);
		tw.Parallel().TweenProperty(nameLabel, "size", nameLabel.Size, 0.5);
		(Material as ShaderMaterial)?.SetShaderParameter("progress", 0);
		nameLabel.Position = nameLabel.Position with { X = nameLabel.Position.X + nameLabel.Size.X };
		nameLabel.Size = nameLabel.Size with { X = 0 };
		SPJ.Meta.PlaySfx("join");
	}

	private void UpdateName()
	{
		nameLabel.Text = Player.username.Get();
	}

	public void UpdateButtons()
	{
		options.GetPopup().SetItemText(1, Player.force_spectator.Get() ? "Unenforce spectator" : "Force spectator");
	}

	private void SpectatorChanged(bool spectator)
	{
		var modulate = spectator ? 0.5f : 1f;
		SelfModulate = SelfModulate with { A = modulate };
		backdrop.SelfModulate = SelfModulate with { A = modulate };
	}

	public void OptionSelected(long id)
	{
		if (id == 0) // Remove
		{
			Player.Kick();
			QueueFree();
		}
		if (id == 1) // Make spectator 
		{
			Player.force_spectator.Set((bool v) => !v);
			UpdateButtons();
		}
	}
}
