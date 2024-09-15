using Godot;
using System;
using static Godot.Tween;

namespace SuperPartyJam.Scenes;

public partial class Transition : Control
{
	[Export] private SPJHeader header;
	[Export] private TextureRect cover_image;
	[Export] private Label loading_text_label;
	[Export] private Label activity_name_label;

	public string activity_name;
	public string loading_text = "Loading";

	public float param = GD.Randf() * Mathf.Pi;

	public override void _Ready()
	{
		loading_text_label.Text = loading_text + loading_text_label.Text;
		cover_image.Position = new Vector2(336, 189) * new Vector2(Mathf.Sin(param), Mathf.Cos(param));
		SPJ.Meta.PlaySfx("transition");
		header.Flash();
		cover_image.Modulate = cover_image.Modulate with { A = 0f };
		var tw = GetTree().CreateTween().SetTrans(TransitionType.Expo).SetEase(EaseType.Out);
		tw.Parallel().TweenProperty(cover_image, "position", Vector2.Zero, 2);
		tw.Parallel().TweenProperty(cover_image, "scale", Vector2.One, 2);
		tw.Parallel().TweenProperty(cover_image, "modulate:a", 1f, 2);
		tw.Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
