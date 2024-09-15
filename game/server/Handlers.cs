using SuperPartyJam.Server.Utils;

namespace SuperPartyJam.Server;

// Boilerplate! Boilerplate everywhere! We love boilerplate!
public partial class SPJClient
{
    private HandlerEventStorage _storage = new HandlerEventStorage();
    public HandlerEventStorage GetStorage() => _storage;
    public event OnUnhandledPacket? UnhandledPacket;
    public void HandleUnhandledPacket(Packet packet) => UnhandledPacket?.Invoke(packet);
}
public partial class SPJPlayer
{
    private HandlerEventStorage _storage = new HandlerEventStorage();
    public HandlerEventStorage GetStorage() => _storage;
    public event OnUnhandledPacket? UnhandledPacket;
    public void HandleUnhandledPacket(Packet packet) => UnhandledPacket?.Invoke(packet);
}
public partial class SPJModule
{
    private HandlerEventStorage _storage = new HandlerEventStorage();
    public HandlerEventStorage GetStorage() => _storage;
    public event OnUnhandledPacket? UnhandledPacket;
    public void HandleUnhandledPacket(Packet packet) => UnhandledPacket?.Invoke(packet);
}