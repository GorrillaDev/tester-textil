namespace AcuratexControlApp;

public sealed class ConnectionController : IDisposable
{
    private IControllerTransport? _transport;

    public bool IsConnected => _transport?.IsConnected == true;

    public event Action<string>? LineReceived;

    public async Task ConnectAsync(ConnectionMode mode, UsbVendorDeviceInfo? device, string host, int tcpPort, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await DisconnectAsync().ConfigureAwait(false);

        _transport = CreateTransport(mode, device, host, tcpPort);
        _transport.LineReceived += HandleLineReceived;
        await _transport.ConnectAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DisconnectAsync()
    {
        if (_transport == null) {
            return;
        }

        try {
            await _transport.DisconnectAsync().ConfigureAwait(false);
        } finally {
            _transport.LineReceived -= HandleLineReceived;
            _transport.Dispose();
            _transport = null;
        }
    }

    public async Task SendLineAsync(string line, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_transport == null || !_transport.IsConnected) {
            throw new InvalidOperationException("No hay conexion activa.");
        }

        await _transport.SendLineAsync(line, cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }

    private static IControllerTransport CreateTransport(ConnectionMode mode, UsbVendorDeviceInfo? device, string host, int tcpPort)
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

    private void HandleLineReceived(string line)
    {
        LineReceived?.Invoke(line);
    }
}
