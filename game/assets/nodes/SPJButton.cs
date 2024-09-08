using Godot;

[GlobalClass]
partial class SPJButton : Button
{
	private float temp_size_x;
	private float temp_global_position_x;

	private Tween? tween = null;
	private float squish_px;

	public override void _Ready()
	{
		MouseEntered += GrabFocus;
		FocusEntered += () => SPJ.Meta.PlaySfx("cursor");
		var clickSound = IsInGroup("cancel") ? "cancel" : "select";
		Pressed += () => SPJ.Meta.PlaySfx(clickSound);
		if (!IsInGroup("static"))
		{
			FocusEntered += Stretch;
			FocusExited += Chill;
			ButtonDown += Squeeze;
			ButtonUp += Chill;
		}
	}

	private void SetSquish(float new_squish)
	{
		var diff = new_squish - squish_px;
		squish_px = new_squish;
		tween = GetTree().CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Circ);
		temp_size_x = Size.X + diff * 2;
		temp_global_position_x = GlobalPosition.X - diff;
		tween.Parallel().TweenProperty(this, "size:x", temp_size_x, 0.2);
		tween.Parallel().TweenProperty(this, "global_position:x", temp_global_position_x, 0.2);
		tween.Play();
	}

	private void Ensure()
	{
		PivotOffset = Size / 2;
		if (tween != null && tween.IsRunning())
		{
			tween?.Stop();
			Size = Size with { X = temp_size_x };
			GlobalPosition = GlobalPosition with { X = temp_global_position_x };
		}
	}

	private void Stretch()
	{
		Ensure();
		SetSquish(20);
	}

	private void Chill()
	{
		Ensure();
		SetSquish(0);
	}

	private void Squeeze()
	{
		Ensure();
		SetSquish(-20);
	}
}