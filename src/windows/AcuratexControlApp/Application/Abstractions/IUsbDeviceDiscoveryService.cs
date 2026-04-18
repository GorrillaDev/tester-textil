using AcuratexControlApp.Shared.Models;

namespace AcuratexControlApp.Application.Abstractions;

public interface IUsbDeviceDiscoveryService
{
    IReadOnlyList<UsbVendorDeviceInfo> GetAvailableDevices();
}
