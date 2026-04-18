using AcuratexControlApp.Application.Models;

namespace AcuratexControlApp.Application.Abstractions;

public interface IConnectionController : IDisposable
{
    bool IsConnected { get; }
    event Action<string>? LineReceived;
    Task ConnectAsync(ConnectionRequest request, CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task SendLineAsync(string line, CancellationToken cancellationToken);
}
