using Godot;
using System;

public partial class LobbyPlayer : Panel
{
	[Export] private OptionButton options;
	[Export] private Label nameLabel;
	[Export] private Control backdrop;

	public SPJPlayer Player;

	public override void _Ready()
	{
		options.GetPopup().IdPressed += OptionSelected;
		UpdateButtons();
		Player.username.Change += (_) => { nameLabel.Text = Player.username.Get(); };
		Player.spectator.Change += SpectatorChanged;
		Player.force_spectator.Change += SpectatorChanged;

	}
	public void Flash()
	{
		UpdateButtons();
		SpectatorChanged();
		var tw = GetTree().CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Circ);
		tw.TweenMethod(Callable.From((float p) => (Material as ShaderMaterial)?.SetShaderParameter("progress", p)), 1.0, 0.115, 0.3);
		tw.Parallel().TweenProperty(nameLabel, "position", nameLabel.Position, 0.5);
		tw.Parallel().TweenProperty(nameLabel, "size", nameLabel.Size, 0.5);
		tw.TweenCallback(Callable.From(() =>
		{
			(Material as ShaderMaterial)?.SetShaderParameter("progress", 0);
			nameLabel.Position = nameLabel.Position with { X = nameLabel.Position.X + nameLabel.Size.X };
			nameLabel.Size = nameLabel.Size with { X = 0 };
			SPJ.Meta.PlaySfx("join");
		}));

	}

	public void UpdateButtons()
	{
		options.GetPopup().SetItemText(1, Player.force_spectator.Get() ? "Unenforce spectator" : "Force spectator");
	}

	private void SpectatorChanged(object _ = null)
	{
		var modulate = Player.IsSpectator() ? 0.5f : 1f;
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
