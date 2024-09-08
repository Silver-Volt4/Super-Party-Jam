using Godot;
using System;

public partial class BeginScreen : Control
{
	[Export] private Label welcomeText;
	[Export] private Control modeSelect;
	[Export] private Label explanation;

	private bool begin = false;
	public event Action PlayLocal;
	public event Action PlayRemote;

	public override void _Input(InputEvent @event)
	{
		if (@event.IsPressed() && !begin)
		{
			SPJ.Meta.PlaySfx("start");
			begin = true;
			var tw = GetTree().CreateTween();
			tw.TweenProperty(welcomeText, "modulate:a", 0, 0.1);
			tw.TweenProperty(modeSelect, "modulate:a", 0, 0);
			tw.TweenCallback(new Callable(modeSelect, Control.MethodName.Show));
			tw.TweenProperty(modeSelect, "modulate:a", 1, 0.1);
			tw.Play();
		}
	}


	public void _on_local_focus_entered()
	{
		explanation.Text = "For parties where everyone is on the same network";
	}

	public void _on_remote_focus_entered()
	{
		explanation.Text = "For remote parties";
	}

	public void _on_local_pressed()
	{
		PlayLocal?.Invoke();
	}

	public void _on_remote_pressed()
	{
		PlayRemote?.Invoke();
	}
}
