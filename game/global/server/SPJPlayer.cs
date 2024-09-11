using Godot;
using Godot.Collections;
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

	[SPJSync(name: "username")] public SPJState<string> username = new("");
	[SPJSync(name: "spectator")] public SPJState<bool> spectator = new(false);
	[SPJSync(name: "force_spectator")] public SPJState<bool> force_spectator = new(false);

	public SPJPlayer()
	{
		Name = Guid.NewGuid().ToString();
	}

	public void SetClient(SPJClient client)
	{
		this.client = client;
		if (!CheckModule()) return;
		this.SetupEventHook();
		connected.Set(true);
		client.Closed += (client) => connected.Set(false);
		client.SendPacket(new SPJPacket
		{
			Type = SPJPacket.PacketType.Call,
			Phase = SPJPacket.PacketPhase.Player,
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
