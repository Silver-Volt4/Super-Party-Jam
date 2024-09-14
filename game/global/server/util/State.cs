using System;
using Godot;

public delegate void OnChange<T>(T new_value);

public class SPJState<[MustBeVariant] T> : SPJRawState<T>
{
    public SPJState(T initial) : base(initial) { }

    public void Set(Variant NewValueVariant)
    {
        T NewValue = NewValueVariant.As<T>();
        Set(NewValue);
    }
}

public class SPJRawState<T>
{
    protected T Value;
    public event OnChange<T> Change;
    public event Action ChangeAction;
    public SPJRawState(T initial) => Value = initial;
    public T Get() => Value;

    public void Set(T NewValue)
    {
        Value = NewValue;
        Change?.Invoke(NewValue);
        ChangeAction?.Invoke();
    }

    public void Set(Func<T, T> transformer)
    {
        Set(transformer(Value));
    }

    public void SetSilently(T NewValue)
    {
        Value = NewValue;
    }
}
