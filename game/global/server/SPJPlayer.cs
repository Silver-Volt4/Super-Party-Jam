using Godot;
using Godot.Collections;
using System;

public partial class SPJPlayer : Node, MPacketHandler
{
	// Events
	public delegate void OnKick(SPJPlayer player);
	public event OnKick KickRequest;

	// End events
	private SPJClient? client = null;
	private SPJModule? module = null;
	private SPJState<bool> connected = new SPJState<bool>(false);

	public enum CloseReason
	{
		Replaced = 4200,
	}

	public PacketPhase GetPhase() => PacketPhase.Player;

	[Sync(name: "username")] public SPJState<string> username = new("");
	[Sync(name: "spectator")] public SPJState<bool> spectator = new(false);
	[Sync(name: "force_spectator")] public SPJState<bool> force_spectator = new(false);

	public SPJPlayer()
	{
		Name = Guid.NewGuid().ToString();
	}

	public void SetClient(SPJClient client)
	{
		this.client = client;
		if (!CheckModule()) return;
		this.InitHandler();
		client.UnhandledPacket += this.HandlePacket;
		connected.Set(true);
		client.Closed += (client) => connected.Set(false);
		client.SendPacket(new Packet
		{
			Type = PacketType.Call,
			Phase = PacketPhase.Player,
			Name = "accepted",
			Data = new Dictionary
			{
				{"username", username.Get()},
				{"token", GetToken()},
			}
		});
		SetModule();
	}

	public void SetModule() => SetModule(module);
	public void SetModule(SPJModule? module)
	{
		this.module = module;
		module?.SetPlayer(this);
		CheckModule();
	}

	private bool CheckModule()
	{
		if (module?.GetName() != client?.GetCurrentModule())
		{
			client.RequestModuleChange(module?.GetName());
			return false;
		}
		return true;
	}

	public bool IsSpectator()
	{
		return spectator.Get() || force_spectator.Get();
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

	public SPJModule GetModule()
	{
		return module;
	}

	public override void _Process(double delta)
	{
		client.Poll();
	}
}
