using System;

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
public class Callable : EventAttribute
{
    public Callable(string name) : base(name)
    {
    }
}