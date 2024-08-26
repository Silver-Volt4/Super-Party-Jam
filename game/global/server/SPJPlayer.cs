using Godot;
using System;

public partial class SPJPlayer : Node, SPJEventHook
{
	private SPJClient? client = null;

	private bool connected = false;

	public SPJPacket.PacketPhase GetPhase()
	{
		return SPJPacket.PacketPhase.Player;
	}

	[SPJSync(name: "username")] public SPJState<string> username = new SPJState<string>("");

	public SPJPlayer()
	{
		this.SetupEventHook();
		Name = Guid.NewGuid().ToString();
	}

	public void SetClient(SPJClient client)
	{
		this.client = client;
		this.SetupEventHook();
		connected = true;
		client.Closed += () => connected = false;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public SPJClient GetClient()
	{
		return this.client;
	}
}
