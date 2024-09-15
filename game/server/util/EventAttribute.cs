using System;

namespace SuperPartyJam.Server.Utils;

public abstract class EventAttribute : Attribute
{
    public string Name;
    public EventAttribute(string name)
    {
        Name = name;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class Sync : EventAttribute
{
    public bool ReadOnly;

    public Sync(string name, bool read_only = false) : base(name)
    {
        ReadOnly = read_only;
    }
}

[AttributeUsage(AttributeTargets.Method)]
public class Call : EventAttribute
{
    public Call(string name) : base(name)
    {
    }
}