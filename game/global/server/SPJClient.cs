using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;

public delegate void ConnectionStateChanged();
public delegate void SPJPacketReceived(SPJPacket packet);

public delegate void OnChange(dynamic new_value);


public class SPJState<T>
{
    private T Value;
    public event OnChange Change;

    public SPJState(T initial) => Value = initial;
    public T Get() => Value;
    public void Set(T NewValue)
    {
        Value = NewValue;
        Change.Invoke(NewValue);
    }
}

public interface SPJEventHook
{
    public SPJClient GetClient();
    public SPJPacket.PacketPhase GetPhase();
}

public static class SPJEventHookExt
{
    public static void SetupEventHook(this SPJEventHook hook)
    {
        var fields = hook.GetType().GetFields();
        var synced =
            from f in fields
            where f.GetCustomAttribute<SPJSync>() != null
            select new { field = f, attribute = f.GetCustomAttribute<SPJSync>() };
        foreach (var sync in synced)
        {
            var field = sync.field;
            var attribute = sync.attribute;
            dynamic state = field.GetValue(hook);
            state.Change += new OnChange((object new_value) => hook.HandleOnChange(attribute.Name, new_value));
        }
    }

    public static void HandleOnChange(this SPJEventHook hook, string name, dynamic new_value)
    {
        var d = new Dictionary<string, Godot.Variant>
        {
            { "phase", Variant.From<int>((int)hook.GetPhase()) },
            { "value", Variant.CreateFrom(new_value) }
        };
        hook.GetClient().Send("sync", d);
    }
}

public abstract class SPJEventAttribute : Attribute
{
    public string Name;
    public SPJEventAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class SPJSync : SPJEventAttribute
{
    public bool ReadOnly;

    public SPJSync(string name, bool read_only = true) : base(name)
    {
        ReadOnly = read_only;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class SPJCallable : SPJEventAttribute
{
    public SPJCallable(string name) : base(name)
    {
    }
}

public class SPJPacket
{
    public enum PacketType
    {
        Sync = 1,
        Call = 2
    }

    public enum PacketPhase
    {
        Client = 1,
        Player = 2,
        Module = 3,
    }

    public static PacketType? TypeFrom(string value)
    {
        return value switch
        {
            "sync" => PacketType.Sync,
            "call" => PacketType.Call,
            _ => null
        };
    }

    public static PacketPhase? PhaseFrom(string value)
    {
        return value switch
        {
            "client" => PacketPhase.Client,
            "player" => PacketPhase.Player,
            "module" => PacketPhase.Module,
            _ => null
        };
    }

    public PacketType Type;
    public PacketPhase Phase;
    public string Name;
    public Dictionary Data;
}

[GlobalClass]
public partial class SPJClient : Resource, SPJEventHook
{
    private WebSocketPeer peer;
    private WebSocketPeer.State peer_state = WebSocketPeer.State.Closed;

    public SPJPacket.PacketPhase GetPhase()
    {
        return SPJPacket.PacketPhase.Client;
    }

    [SPJSync(name: "active")] public SPJState<bool> active = new SPJState<bool>(false);

    public event ConnectionStateChanged Connected;
    public event ConnectionStateChanged Closed;
    public event SPJPacketReceived PacketReceived;

    public SPJClient(
        WebSocketPeer peer
    )
    {
        this.peer = peer;
        GD.Print(this.active.GetType());
        this.SetupEventHook();
        this.active.Set(true);
    }


    public void Send(string event_name, Godot.Collections.Dictionary<string, Variant> data)
    {
        data.Add("event", event_name);
        var d = Json.Stringify(data);
        peer.SendText(d);
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
                Connected?.Invoke();
            }
            else if (state == WebSocketPeer.State.Closed)
            {
                Closed?.Invoke();
            }
        }

        while (peer.GetAvailablePacketCount() > 0)
        {
            var packet = peer.GetPacket();
            if (!peer.WasStringPacket()) continue;
            var data = Json.ParseString(packet.GetStringFromUtf8()).As<Dictionary>();
            ProcessPacket(data);
        }
    }

    private void ProcessPacket(Dictionary data)
    {
        try
        {
            var packet = new SPJPacket
            {
                Type = SPJPacket.TypeFrom(data["type"].AsString()) ?? throw new Exception(),
                Phase = SPJPacket.PhaseFrom(data["phase"].AsString()) ?? throw new Exception(),
                Name = data["name"].AsString() ?? throw new Exception()
            };
            data.Remove("type");
            data.Remove("phase");
            data.Remove("name");
            packet.Data = data;
            PacketReceived.Invoke(packet);
        }
        catch
        {

        }
    }

    [SPJCallable(name: "register")]
    protected void Register()
    {
        GD.Print("dummy");
    }


    SPJClient SPJEventHook.GetClient()
    {
        return this;
    }
}
