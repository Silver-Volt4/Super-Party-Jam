using Godot;
using System;
using System.Collections.Generic;

public partial class SPJGameServer : Node
{
	// Events
	public delegate void OnNewPlayer(SPJPlayer player);
	public event OnNewPlayer NewPlayer;
	// End events

	private TcpServer server = new TcpServer();
	private List<SPJClient> clients = new List<SPJClient>();

	public override void _Ready()
	{
		base._Ready();
		SPJ.GameServer = this;
	}

	public void Start()
	{
		SPJHelpers.RunServer(server, 12004);
	}

	public int GetPort()
	{
		return server.GetLocalPort();
	}

	public override void _Process(double delta)
	{
		while (server.IsConnectionAvailable())
		{
			var peer = new WebSocketPeer();
			peer.InboundBufferSize = 134215680;
			peer.AcceptStream(server.TakeConnection());
			var client = new SPJClient(peer);
			client.Closed += RemoveClient;
			client.Activate += ActivateClient;
			clients.Add(client);
		}

		foreach (var client in clients)
		{
			client.Poll();
		}
	}

	private void ActivateClient(SPJClient client, string username, string? token)
	{
		if (token == null)
		{
			var player = new SPJPlayer();
			player.SetClient(client);
			player.username.Set(username);
			player.KickRequest += KickPlayer;
			AddChild(player);
			RemoveClient(client);
			NewPlayer?.Invoke(player);
		}
		else
		{
			var player = GetNodeOrNull(token) as SPJPlayer;
			if (player != null)
			{
				RemoveClient(client);
				player.SetClient(client);
			}
			else
			{
				client.Close((int)SPJClient.CloseReason.InvalidToken);
			}
		}
	}

	private void KickPlayer(SPJPlayer player)
	{
		player.GetClient().Close((int)SPJClient.CloseReason.RemovedByHost);
		player.QueueFree();
	}

	private void RemoveClient(SPJClient client)
	{
		clients.Remove(client);
	}
}
