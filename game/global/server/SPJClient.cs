using System;
using Godot;
using Godot.Collections;
using System.Collections.Generic;

[GlobalClass]
public partial class SPJClient : Resource, SPJEventHook
{
    // Events
    public delegate void OnConnectionStateChanged(SPJClient client);
    public delegate void OnPacketReceived(SPJClient client, SPJPacket packet);
    public delegate void OnActivate(SPJClient client, string username, string? token);
    public event OnConnectionStateChanged Connected;
    public event OnConnectionStateChanged Closed;
    public event OnPacketReceived PacketReceived;
    public event OnActivate Activate;
    // End events
    private WebSocketPeer peer;
    private WebSocketPeer.State peer_state = WebSocketPeer.State.Closed;
    private SPJStorage _storage = new SPJStorage();
    private string? module = null;

    public enum CloseReason
    {
        InvalidToken = 4100,
        RemovedByHost = 4101,
    }

    public SPJPacket.PacketPhase GetPhase() => SPJPacket.PacketPhase.Client;

    public SPJStorage GetStorage() => _storage;

    [SPJSync("active", read_only: true)] public SPJState<bool> active = new SPJState<bool>(false);

    public SPJClient(
        WebSocketPeer peer
    )
    {
        this.peer = peer;
        this.SetupEventHook();
    }

    public void SendPacket(SPJPacket packet)
    {
        Dictionary data = packet.Data;
        data.Add("type", (int)packet.Type);
        data.Add("phase", (int)packet.Phase);
        data.Add("name", packet.Name);
        peer.SendText(Json.Stringify(data));
    }

    public void Send(string data)
    {
        peer.SendText(data);
    }

    public void Close(int code, string reason = "")
    {
        peer.Close(code, reason);
    }

    public void Poll()
    {
        peer.Poll();

        var state = peer.GetReadyState();
        if (peer_state != state)
        {
            peer_state = state;
            if (state == WebSocketPeer.State.Open)
            {
                Connected?.Invoke(this);
            }
            else if (state == WebSocketPeer.State.Closed)
            {
                Closed?.Invoke(this);
            }
        }

        while (peer.GetAvailablePacketCount() > 0)
        {
            var packet = peer.GetPacket();
            if (!peer.WasStringPacket()) continue;
            var data = Json.ParseString(packet.GetStringFromUtf8()).As<Dictionary>();
            CreateAndProcessPacket(data);
        }
    }

    private void CreateAndProcessPacket(Dictionary data)
    {
        try
        {
            var packet = new SPJPacket
            {
                Type = (SPJPacket.PacketType)data["type"].As<int>(),
                Phase = (SPJPacket.PacketPhase)data["phase"].As<int>(),
                Name = data["name"].AsString() ?? throw new Exception()
            };
            data.Remove("type");
            data.Remove("phase");
            data.Remove("name");
            packet.Data = data;
            PacketReceived?.Invoke(this, packet);
        }
        catch
        {
        }
    }

    [SPJCallable(name: "register")]
    public void Register(SPJPacket packet)
    {
        Variant username;
        if (packet.Data.TryGetValue("username", out username)) return;
        Variant? token = packet.Data.GetValueOrDefault("token");
        module = packet.Data.GetValueOrDefault("module").AsString();
        Activate?.Invoke(this, username.AsString(), token?.AsString());
    }

    public string? GetCurrentModule() => module;
    SPJClient SPJEventHook.GetClient() => this;
}
