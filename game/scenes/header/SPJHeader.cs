using Godot;
using System;
using System.Threading.Tasks;

namespace SuperPartyJam.Scenes;

public partial class SPJHeader : Control
{
	[Export] private Control header;
	[Export] public Label label;
	[Export] private Control flasher;

	private void FlasherProgress(float p)
	{
		var material = flasher.Material as ShaderMaterial;
		material?.SetShaderParameter("progress", p);
	}

	public async void Flash(bool show = true)
	{
		header.Visible = !show;
		label.Visible = !show;
		var tw = CreateTween().SetEase(Tween.EaseType.InOut).SetTrans(Tween.TransitionType.Circ);
		var callbackFlasherProgress = (float p) => { FlasherProgress(p); };
		tw.TweenMethod(Godot.Callable.From(callbackFlasherProgress), 0.0, 1.0, 0.15);
		tw.TweenCallback(Godot.Callable.From(() => header.Visible = show));
		tw.TweenCallback(Godot.Callable.From(() => label.Visible = show));
		tw.TweenMethod(Godot.Callable.From(callbackFlasherProgress), 1.0, 2.0, 0.15);
		await ToSignal(tw, Tween.SignalName.Finished);
	}
}
