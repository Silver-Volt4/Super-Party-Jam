using Godot;
using System;

public partial class ModuleButton : Button
{
	[Export] private TextureRect thumbnail;

	private Tween? tween = null;

	public SPJModuleMetadata metadata;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		thumbnail.Texture = metadata.Thumbnail;
		MouseEntered += GrabFocus;
		FocusEntered += Focus;
		FocusExited += Unfocus;
		ButtonDown += Press;
		ButtonUp += Unpress;
	}

	private void AnimHover(float anim)
	{
		thumbnail.PivotOffset = Size / 2;
		thumbnail.Scale = Vector2.One.Lerp(Vector2.One * 1.15f, anim);
		thumbnail.RotationDegrees = Mathf.Lerp(0f, -4f, anim);
		PivotOffset = Size / 2;
		Scale = Vector2.One.Lerp(Vector2.One * 1.05f, anim);
	}

	private void AnimPress(float anim)
	{
		PivotOffset = Size / 2;
		Scale = (Vector2.One * 1.05f).Lerp(Vector2.One * 0.85f, anim);
	}

	private void InitTween()
	{
		tween?.Stop();
		tween = GetTree().CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Circ);
	}

	private void Focus()
	{
		SPJ.Meta.PlaySfx("cursor");
		InitTween();
        tween.TweenMethod(new Godot.Callable(this, "AnimHover"), 0.0, 1.0, 0.1);
	}

	private void Unfocus()
	{
		InitTween();
        tween.TweenMethod(new Godot.Callable(this, "AnimHover"), 1.0, 0.0, 0.1);
	}

	private void Press()
	{
		InitTween();
        tween.TweenMethod(new Godot.Callable(this, "AnimPress"), 0.0, 1.0, 0.1);
	}

	private void Unpress()
	{
		InitTween();
        tween.TweenMethod(new Godot.Callable(this, "AnimPress"), 1.0, 0.0, 0.1);
	}
}
