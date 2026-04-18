using AcuratexControlApp.Application.Models;
using AcuratexControlApp.Shared.Models;

namespace AcuratexControlApp.Application.Abstractions;

public interface IDeviceControlService : IDisposable
{
    bool IsConnected { get; }
    event Action<string>? LineReceived;

    IReadOnlyList<UsbVendorDeviceInfo> GetUsbDevices();
    Task ConnectAsync(ConnectionRequest request, CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task SendLineAsync(string line, CancellationToken cancellationToken);
}
