namespace AcuratexControlApp;

public sealed class ControllerTransportFactory
{
    public IControllerTransport Create(ConnectionMode mode, UsbVendorDeviceInfo? device, string host, int tcpPort)
    {
        if (mode == ConnectionMode.Usb) {
            if (device == null) {
                throw new InvalidOperationException("Selecciona un dispositivo USB Acuratex.");
            }

            return new WinUsbControllerTransport(device.DevicePath);
        }

        if (string.IsNullOrWhiteSpace(host)) {
            throw new InvalidOperationException("Host invalido.");
        }

        if (tcpPort <= 0) {
            throw new InvalidOperationException("Puerto TCP invalido.");
        }

        return new TcpControllerTransport(host, tcpPort);
    }
}
