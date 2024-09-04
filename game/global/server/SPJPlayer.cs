using Godot;
using System;

public partial class SPJPlayer : Node, SPJEventHook
{
	// Events
	public delegate void OnKick(SPJPlayer player);
	public event OnKick KickRequest;
	// End events
	private SPJClient? client = null;
	private SPJModule? module = null;
	private SPJState<bool> connected = new SPJState<bool>(false);
	private SPJStorage _storage = new SPJStorage();

	public enum CloseReason
	{
		Replaced = 4200,
	}

	public SPJPacket.PacketPhase GetPhase() => SPJPacket.PacketPhase.Player;

	public SPJStorage GetStorage() => _storage;

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
		connected.Set(true);
		client.Closed += (client) => connected.Set(false);
	}

	public void SetModule(SPJModule module)
	{
		this.module = module;
		module.SetPlayer(this);
	}

	public string GetToken() => Name;

	public void Kick()
	{
		KickRequest?.Invoke(this);
	}

	public SPJClient GetClient()
	{
		return client;
	}

	public override void _Process(double delta)
	{
		client.Poll();
	}
}
