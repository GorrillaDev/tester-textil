namespace AcuratexControlApp;

public interface IConnectionController : IDisposable
{
    bool IsConnected { get; }
    event Action<string>? LineReceived;
    Task ConnectAsync(ConnectionMode mode, UsbVendorDeviceInfo? device, string host, int tcpPort, CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task SendLineAsync(string line, CancellationToken cancellationToken);
}
