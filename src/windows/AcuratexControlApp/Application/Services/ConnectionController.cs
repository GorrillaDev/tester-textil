using AcuratexControlApp.Application.Abstractions;
using AcuratexControlApp.Application.Models;

namespace AcuratexControlApp.Application.Services;

public sealed class ConnectionController : IConnectionController
{
    private readonly IControllerTransportFactory _transportFactory;
    private IControllerTransport? _transport;

    public ConnectionController(IControllerTransportFactory transportFactory)
    {
        _transportFactory = transportFactory ?? throw new ArgumentNullException(nameof(transportFactory));
    }

    public bool IsConnected => _transport?.IsConnected == true;

    public event Action<string>? LineReceived;

    public async Task ConnectAsync(ConnectionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        await DisconnectAsync().ConfigureAwait(false);

        _transport = _transportFactory.Create(request);
        _transport.LineReceived += HandleLineReceived;

        try {
            await _transport.ConnectAsync(cancellationToken).ConfigureAwait(false);
        } catch {
            _transport.LineReceived -= HandleLineReceived;
            _transport.Dispose();
            _transport = null;
            throw;
        }
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
