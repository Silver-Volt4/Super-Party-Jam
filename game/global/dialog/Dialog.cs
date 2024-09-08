using Godot;
using Microsoft.AspNetCore.Http;
using System;

public delegate void OnResolve(bool accept);
public partial class Dialog : Control
{
	[Export] private Button button_ok;
	[Export] private Button button_cancel;
	[Export] private Button button_dismiss;
	[Export] private VBoxContainer dialog_content;
	[Export] private SPJHeader dialog_header;
	[Export] private Control frosted_glass;
	[Export] private Control dimming;
	[Export] private Label dialog_text;

	public event OnResolve Resolve;

	public override async void _Ready()
	{
		button_dismiss.CallDeferred(Button.MethodName.GrabFocus);
		Resolve += FadeOut;
		dialog_content.Size = dialog_content.Size with { Y = dialog_header.Size.Y };
		dialog_content.Position = dialog_content.Position with { Y = (Size.Y - dialog_header.Size.Y) / 2 };

		frosted_glass.SelfModulate = frosted_glass.SelfModulate with { A = 0f };
		dimming.SelfModulate = dimming.SelfModulate with { A = 0f };

		button_ok.Pressed += () => { Resolve?.Invoke(true); };
		button_dismiss.Pressed += () => { Resolve?.Invoke(true); };
		button_cancel.Pressed += () => { Resolve?.Invoke(false); };

		dialog_header.Flash();
		var tw = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Circ);
		tw.TweenProperty(frosted_glass, "self_modulate:a", 1, 0.3);
		tw.Parallel().TweenProperty(dimming, "self_modulate:a", 1, 0.3);
		tw.TweenProperty(dialog_content, "size:y", 650, 0.3);
		tw.Parallel().TweenProperty(dialog_content, "position:y", (Size.Y - 650) / 2, 0.3);
		tw.Parallel().TweenCallback(Callable.From(() => SPJ.Meta.PlaySfx("dialog")));
		tw.TweenCallback(Callable.From(() => button_dismiss.Disabled = false));
		tw.Parallel().TweenCallback(Callable.From(() => button_ok.Disabled = false));
		tw.Parallel().TweenCallback(Callable.From(() => button_cancel.Disabled = false));
	}

	public Dialog Title(string title_text)
	{
		dialog_header.label.Text = title_text;
		return this;
	}

	public Dialog Text(string text)
	{
		dialog_text.Text = text;
		return this;
	}

	public Dialog Alert(string dismiss_text)
	{
		button_dismiss.Show();
		button_ok.Hide();
		button_cancel.Hide();
		button_dismiss.Text = dismiss_text;
		return this;
	}

	public Dialog Decide(string ok_text, string cancel_text)
	{
		button_dismiss.Hide();
		button_ok.Show();
		button_cancel.Show();
		button_ok.Text = ok_text;
		button_cancel.Text = cancel_text;
		return this;
	}

	private async void FadeOut(bool resolve)
	{
		var tw = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Circ);
		tw.TweenProperty(dialog_content, "size:y", dialog_header.Size.Y, 0.1);
		tw.Parallel().TweenProperty(dialog_content, "position:y", (Size.Y - dialog_header.Size.Y) / 2, 0.2);
		tw.Parallel().TweenCallback(Callable.From(() => dialog_header.Flash(false)));
		tw.TweenProperty(frosted_glass, "self_modulate:a", 0, 0.3);
		tw.Parallel().TweenProperty(dimming, "self_modulate:a", 0, 0.3);
		tw.Finished += QueueFree;
	}

	// 	var tw = create_tween().set_ease(Tween.EASE_IN_OUT).set_trans(Tween.TRANS_CIRC)
	// tw.tween_property($Dialog, "size:y", $Dialog/Header.size.y, 0.1)
	// tw.parallel().tween_property($Dialog, "position:y", (self.size.y - $Dialog/Header.size.y)/2, 0.2)
	// $Dialog/Header.flash(false)
	// tw.tween_property($FrostedGlass, "self_modulate:a", 0, 0.3)
	// tw.parallel().tween_property($Dimming, "self_modulate:a", 0, 0.3)
	// tw.finished.connect(self.queue_free)
}
