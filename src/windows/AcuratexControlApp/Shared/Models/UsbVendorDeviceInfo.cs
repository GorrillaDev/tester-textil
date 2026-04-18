namespace AcuratexControlApp.Shared.Models;

public sealed record UsbVendorDeviceInfo(string DevicePath)
{
    public override string ToString()
    {
        return DevicePath;
    }
}
