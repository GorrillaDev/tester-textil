using AcuratexControlApp.Application.Abstractions;
using AcuratexControlApp.Application.Models;
using AcuratexControlApp.Infrastructure.Transports;

namespace AcuratexControlApp.Infrastructure.Factories;

public sealed class ControllerTransportFactory : IControllerTransportFactory
{
    public IControllerTransport Create(ConnectionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request switch {
            UsbConnectionRequest usbRequest => CreateUsbTransport(usbRequest),
            WifiConnectionRequest wifiRequest => CreateWifiTransport(wifiRequest),
            _ => throw new NotSupportedException($"Modo de conexion no soportado: {request.GetType().Name}."),
        };
    }

    private static IControllerTransport CreateUsbTransport(UsbConnectionRequest request)
    {
        if (request.Device == null) {
            throw new InvalidOperationException("Selecciona un dispositivo USB Acuratex.");
        }

        return new WinUsbControllerTransport(request.Device.DevicePath);
    }

    private static IControllerTransport CreateWifiTransport(WifiConnectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Host)) {
            throw new InvalidOperationException("Host invalido.");
        }

        if (request.Port <= 0) {
            throw new InvalidOperationException("Puerto TCP invalido.");
        }

        return new TcpControllerTransport(request.Host.Trim(), request.Port);
    }
}
