using Godot;

public static class SPJ {
	public static SPJMeta Meta;

    public static SPJGameServer GameServer;
    public static SPJHttpServer HttpServer;

}

public partial class SPJMeta : Node
{
	[Export] private CanvasLayer overlays;

	public override void _Ready()
	{
		base._Ready();
		SPJ.Meta = this;
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event.IsActionPressed("fullscreen"))
		{
			GetWindow().Mode = GetWindow().Mode == Window.ModeEnum.ExclusiveFullscreen ? Window.ModeEnum.Windowed : Window.ModeEnum.ExclusiveFullscreen;
		}
	}

	public void PlaySfx(string sound)
	{
		GetNode<AudioStreamPlayer>(sound).Play();
	}

	public SPJModule LoadModuleClass(string name)
	{
		return GD.Load<CSharpScript>($"res://games/{name}/{name.Capitalize()}Module.cs").New().As<SPJModule>();
	}

	// public void switch_module(string name, SPJPlayer[] players)
	// {
	// 	var module = load_module_class(name)
	// 	for p in players:
	// 		p.module = module.new()
	// 	self.__players = players
	// 	get_tree().change_scene_to_file.call_deferred("res://games/" + name + "/root.tscn")
	// 	}

	// public void get_players()
	// {
	// 	return __players
	// 	}

	public void Alert(string title, string text)
	{
		var dialog = GD.Load<PackedScene>("res://global/dialog/dialog.tscn").Instantiate<Dialog>();
		overlays.AddChild(dialog.Title(title).Text(text).Alert("OK"));
	}
}