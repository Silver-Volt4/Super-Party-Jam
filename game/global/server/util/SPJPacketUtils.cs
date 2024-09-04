using System;
using NativeDictionary = System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot.Collections;
using System.Collections.Generic;

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

public class SPJStorage
{
    private NativeDictionary.Dictionary<SPJPacket.PacketType, NativeDictionary.Dictionary<string, dynamic>> storage = new NativeDictionary.Dictionary<SPJPacket.PacketType, NativeDictionary.Dictionary<string, dynamic>>();

    private NativeDictionary.Dictionary<string, dynamic> GetPacketStorage(SPJPacket.PacketType packetType)
    {
        var has = storage.ContainsKey(packetType);
        if (!has)
        {
            var dict = new NativeDictionary.Dictionary<string, dynamic>();
            storage.Add(packetType, dict);
            return dict;
        }
        return storage[packetType];
    }

    public void Add(SPJPacket.PacketType packetType, string name, dynamic value)
    {
        var packetStorage = GetPacketStorage(packetType);
        packetStorage.Add(name, value);
    }

    public dynamic? Get(SPJPacket.PacketType packetType, string name)
    {
        return GetPacketStorage(packetType).GetValueOrDefault(name);
    }

    public NativeDictionary.Dictionary<SPJPacket.PacketType, NativeDictionary.Dictionary<string, dynamic>> GetDictionary() => storage;
}

public interface SPJEventHook
{
    public SPJClient GetClient();
    public SPJStorage GetStorage();
    public SPJPacket.PacketPhase GetPhase();
}

public static class SPJEventHookExt
{
    public static void SetupEventHook(this SPJEventHook hook)
    {
        var syncAttributes =
            from f in hook.GetType().GetFields()
            where f.GetCustomAttribute<SPJSync>() != null
            select new { field = f, attribute = f.GetCustomAttribute<SPJSync>() };

        foreach (var sync in syncAttributes)
        {
            var field = sync.field;
            var attribute = sync.attribute;
            dynamic state = field.GetValue(hook);
            if (!attribute.ReadOnly)
            {
                hook.GetStorage().Add(SPJPacket.PacketType.Sync, attribute.Name, state);
            }
            state.Change += new OnChange((object new_value) => hook.HandleOnChange(attribute.Name, new_value));
        }

        var callableAttributes =
            from m in hook.GetType().GetMethods()
            where m.GetCustomAttribute<SPJCallable>() != null
            select new { method = m, attribute = m.GetCustomAttribute<SPJCallable>() };

        foreach (var callable in callableAttributes)
        {
            var method = callable.method;
            var attribute = callable.attribute;
            var call = (SPJPacket packet) => method.Invoke(hook, new object[] { packet });
            hook.GetStorage().Add(SPJPacket.PacketType.Call, attribute.Name, call);
        }

        hook.GetClient().PacketReceived += hook.HandlePacket;
    }

    private static void HandleOnChange(this SPJEventHook hook, string name, dynamic new_value)
    {
        Dictionary data = new Dictionary
        {
            { "value", new_value }
        };
        hook.GetClient().SendPacket(new SPJPacket
        {
            Type = SPJPacket.PacketType.Sync,
            Phase = hook.GetPhase(),
            Name = name,
            Data = data
        });

    }

    private static void HandlePacket(this SPJEventHook hook, SPJClient client, SPJPacket packet)
    {
        if (packet.Phase != hook.GetPhase()) return;
        var subject = hook.GetStorage().Get(packet.Type, packet.Name);
        if (subject == null) return;

        if (packet.Type == SPJPacket.PacketType.Sync)
        {
            subject.Set(packet.Data.GetValueOrDefault("value"));
        }
        else if (packet.Type == SPJPacket.PacketType.Call)
        {
            subject(packet);
        }
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

    public SPJSync(string name, bool read_only = false) : base(name)
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

    public PacketType Type;
    public PacketPhase Phase;
    public string Name;
    public Dictionary Data;
}