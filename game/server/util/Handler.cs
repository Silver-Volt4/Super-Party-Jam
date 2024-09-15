using System;
using System.Linq;
using System.Reflection;
using Godot.Collections;
using System.Collections.Generic;

namespace SuperPartyJam.Server.Utils;

using EventTypeStorage = System.Collections.Generic.Dictionary<PacketType, System.Collections.Generic.Dictionary<string, dynamic>>;
using EventNameStorage = System.Collections.Generic.Dictionary<string, dynamic>;

public class HandlerEventStorage
{
    private EventTypeStorage storage = new();

    private EventNameStorage GetPacketStorage(PacketType packetType)
    {
        var has = storage.ContainsKey(packetType);
        if (!has)
        {
            EventNameStorage dict = new();
            storage.Add(packetType, dict);
            return dict;
        }
        return storage[packetType];
    }

    public void Clear()
    {
        storage.Clear();
    }

    public void Add(PacketType packetType, string name, dynamic value)
    {
        var packetStorage = GetPacketStorage(packetType);
        packetStorage.Add(name, value);
    }

    public dynamic? Get(PacketType packetType, string name)
    {
        return GetPacketStorage(packetType).GetValueOrDefault(name);
    }

    public EventTypeStorage GetDictionary() => storage;
}

public delegate void OnUnhandledPacket(Packet packet);
public interface MPacketHandler
{
    public SPJClient GetClient();
    public HandlerEventStorage GetStorage();
    public PacketPhase GetPhase();
    public event OnUnhandledPacket UnhandledPacket;
    public void HandleUnhandledPacket(Packet packet);
}

public static class PacketHandler
{
    public static void InitHandler(this MPacketHandler hook)
    {
        hook.GetStorage().Clear();
        var syncAttributes =
            from f in hook.GetType().GetFields()
            where f.GetCustomAttribute<Sync>() != null
            select new { field = f, attribute = f.GetCustomAttribute<Sync>() };

        foreach (var sync in syncAttributes)
        {
            var field = sync.field;
            var attribute = sync.attribute;
            dynamic state = field.GetValue(hook);
            if (!attribute.ReadOnly)
            {
                hook.GetStorage().Add(PacketType.Sync, attribute.Name, state);
            }
            state.ChangeAction += new Action(() => HandleOnChange(hook, attribute.Name, state.Get()));
        }

        var callableAttributes =
            from m in hook.GetType().GetMethods()
            where m.GetCustomAttribute<Call>() != null
            select new { method = m, attribute = m.GetCustomAttribute<Call>() };

        foreach (var callable in callableAttributes)
        {
            var method = callable.method;
            var attribute = callable.attribute;
            var call = (Packet packet) => method.Invoke(hook, new object[] { packet });
            hook.GetStorage().Add(PacketType.Call, attribute.Name, call);
        }
    }

    private static void HandleOnChange(this MPacketHandler hook, string name, dynamic new_value)
    {
        Dictionary data = new Dictionary
        {
            { "value", new_value }
        };
        hook.GetClient().SendPacket(new Packet
        {
            Type = PacketType.Sync,
            Phase = hook.GetPhase(),
            Name = name,
            Data = data
        });
    }

    public static void HandlePacket(this MPacketHandler hook, Packet packet)
    {
        var subject = hook.GetStorage().Get(packet.Type, packet.Name);

        if (subject == null)
        {
            hook.HandleUnhandledPacket(packet);
            return;
        }

        if (packet.Type == PacketType.Sync)
        {
            subject.Set(packet.Data.GetValueOrDefault("value"));
        }
        else if (packet.Type == PacketType.Call)
        {
            subject(packet);
        }
    }
}