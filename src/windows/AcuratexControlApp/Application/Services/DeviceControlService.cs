using AcuratexControlApp.Application.Abstractions;
using AcuratexControlApp.Application.Models;
using AcuratexControlApp.Shared.Models;

namespace AcuratexControlApp.Application.Services;

public sealed class DeviceControlService : IDeviceControlService
{
    private readonly IConnectionController _connectionController;
    private readonly IUsbDeviceDiscoveryService _usbDeviceDiscoveryService;

    public DeviceControlService(
        IConnectionController connectionController,
        IUsbDeviceDiscoveryService usbDeviceDiscoveryService)
    {
        _connectionController = connectionController ?? throw new ArgumentNullException(nameof(connectionController));
        _usbDeviceDiscoveryService = usbDeviceDiscoveryService ?? throw new ArgumentNullException(nameof(usbDeviceDiscoveryService));
    }

    public bool IsConnected => _connectionController.IsConnected;

    public event Action<string>? LineReceived
    {
        add => _connectionController.LineReceived += value;
        remove => _connectionController.LineReceived -= value;
    }

    public IReadOnlyList<UsbVendorDeviceInfo> GetUsbDevices()
    {
        return _usbDeviceDiscoveryService
            .GetAvailableDevices()
            .OrderBy(static device => device.DevicePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public Task ConnectAsync(ConnectionRequest request, CancellationToken cancellationToken)
    {
        return _connectionController.ConnectAsync(request, cancellationToken);
    }

    public Task DisconnectAsync()
    {
        return _connectionController.DisconnectAsync();
    }

    public Task SendLineAsync(string line, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(line)) {
            throw new InvalidOperationException("El comando no puede estar vacio.");
        }

        return _connectionController.SendLineAsync(line.Trim(), cancellationToken);
    }

    public void Dispose()
    {
        _connectionController.Dispose();
    }
}
