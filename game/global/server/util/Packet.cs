using Godot.Collections;

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

public class Packet
{
    public PacketType Type;
    public PacketPhase Phase;
    public string Name;
    public Dictionary Data;
}