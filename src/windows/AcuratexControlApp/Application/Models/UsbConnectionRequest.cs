using AcuratexControlApp.Shared.Models;

namespace AcuratexControlApp.Application.Models;

public sealed record UsbConnectionRequest(UsbVendorDeviceInfo Device)
    : ConnectionRequest(ConnectionMode.Usb);
