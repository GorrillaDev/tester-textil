namespace AcuratexControlApp;

public sealed class UsbVendorDeviceInfo
{
    public UsbVendorDeviceInfo(string devicePath)
    {
        DevicePath = devicePath;
    }

    public string DevicePath { get; }

    public override string ToString()
    {
        return DevicePath;
    }
}
