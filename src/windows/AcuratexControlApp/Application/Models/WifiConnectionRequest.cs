using AcuratexControlApp.Shared.Models;

namespace AcuratexControlApp.Application.Models;

public sealed record WifiConnectionRequest(string Host, int Port)
    : ConnectionRequest(ConnectionMode.Wifi);
