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
        peer.SendText(Json.Stringify(new Dictionary()
        {
            {"type", (int)packet.Type},
            {"phase", (int)packet.Phase},
            {"name", packet.Name},
            {"data", packet.Data}
        }));
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
        var fuckVariants = data.GetValueOrDefault("name");
        var packet = new SPJPacket
        {
            Type = (SPJPacket.PacketType)data.GetValueOrDefault("type").AsInt32(),
            Phase = (SPJPacket.PacketPhase)data.GetValueOrDefault("phase").AsInt32(),
            Name = data.GetValueOrDefault("name").AsString(),
            Data = data.GetValueOrDefault("data").AsGodotDictionary()
        };
        PacketReceived?.Invoke(this, packet);
    }

    [SPJCallable(name: "register")]
    public void Register(SPJPacket packet)
    {
        string? username = null;
        string? token = null;
        if (packet.Data.TryGetValue("username", out var packet_username)) username = packet_username.ToString();
        if (packet.Data.TryGetValue("token", out var packet_token)) token = packet_token.ToString();
        if (packet.Data.TryGetValue("module", out var module)) this.module = module.AsString();
        Activate?.Invoke(this, username, token);
    }

    public string? GetCurrentModule() => module;
    SPJClient SPJEventHook.GetClient() => this;

    public void RequestModuleChange(string? module_name)
    {
        if (module_name != null)
        {
            Close((int)SPJModule.CloseReason.SwitchToModule, module_name);
        }
        else
        {
            Close((int)SPJModule.CloseReason.ClearModule);
        }
    }
}
