namespace AcuratexControlApp;

public sealed class ConnectionController : IConnectionController
{
    private IControllerTransport? _transport;
    private readonly ControllerTransportFactory _transportFactory;

    public ConnectionController()
        : this(new ControllerTransportFactory())
    {
    }

    public ConnectionController(ControllerTransportFactory transportFactory)
    {
        _transportFactory = transportFactory;
    }

    public bool IsConnected => _transport?.IsConnected == true;

    public event Action<string>? LineReceived;

    public async Task ConnectAsync(ConnectionMode mode, UsbVendorDeviceInfo? device, string host, int tcpPort, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await DisconnectAsync().ConfigureAwait(false);

        _transport = _transportFactory.Create(mode, device, host, tcpPort);
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

    private void HandleLineReceived(string line)
    {
        LineReceived?.Invoke(line);
    }
}
