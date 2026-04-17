namespace AcuratexControlApp;

public static class WinUsbDeviceEnumerator
{
    public static IReadOnlyList<UsbVendorDeviceInfo> Enumerate(Guid interfaceGuid)
    {
        Guid guid = interfaceGuid;
        int status = WinUsbNative.CM_Get_Device_Interface_List_SizeW(
            out uint bufferLength,
            ref guid,
            null,
            WinUsbNative.CM_GET_DEVICE_INTERFACE_LIST_PRESENT);

        if (status != WinUsbNative.CR_SUCCESS || bufferLength <= 1) {
            return Array.Empty<UsbVendorDeviceInfo>();
        }

        char[] buffer = new char[bufferLength];
        status = WinUsbNative.CM_Get_Device_Interface_ListW(
            ref guid,
            null,
            buffer,
            bufferLength,
            WinUsbNative.CM_GET_DEVICE_INTERFACE_LIST_PRESENT);

        if (status != WinUsbNative.CR_SUCCESS) {
            return Array.Empty<UsbVendorDeviceInfo>();
        }

        List<UsbVendorDeviceInfo> devices = new();
        int startIndex = 0;

        for (int i = 0; i < buffer.Length; i++) {
            if (buffer[i] != '\0') {
                continue;
            }

            if (i == startIndex) {
                break;
            }

            string devicePath = new(buffer, startIndex, i - startIndex);
            devices.Add(new UsbVendorDeviceInfo(devicePath));
            startIndex = i + 1;
        }

        return devices;
    }
}
