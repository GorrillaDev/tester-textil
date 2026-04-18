using AcuratexControlApp.Application.Abstractions;
using AcuratexControlApp.Infrastructure.Usb;
using AcuratexControlApp.Shared.Constants;
using AcuratexControlApp.Shared.Models;

namespace AcuratexControlApp.Infrastructure.Discovery;

public sealed class WinUsbDeviceDiscoveryService : IUsbDeviceDiscoveryService
{
    public IReadOnlyList<UsbVendorDeviceInfo> GetAvailableDevices()
    {
        return WinUsbDeviceEnumerator.Enumerate(AcuratexUsbConstants.InterfaceGuid);
    }
}
