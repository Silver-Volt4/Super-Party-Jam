using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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
		SPJ.RunServer(server, 12004);
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
			var client = new SPJClient(peer);
			client.Closed += RemoveClient;
			client.Activate += ActivateClient;
			peer.AcceptStream(server.TakeConnection());
			clients.Add(client);
		}

		for (int i = clients.Count() - 1; i >= 0; i--)
		{
			clients[i].Poll();
		}
	}

	private void ActivateClient(SPJClient client, string? username, string? token)
	{
		if (token == null && username != null)
		{
			var player = new SPJPlayer();
			player.username.SetSilently(username);
			player.SetClient(client);
			player.KickRequest += KickPlayer;
			AddChild(player);
			RemoveClient(client);
			NewPlayer?.Invoke(player);
		}
		else if (token != null)
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

	public SPJPlayer[] GetPlayers()
	{
		return GetChildren().OfType<SPJPlayer>().ToArray();
	}
}
