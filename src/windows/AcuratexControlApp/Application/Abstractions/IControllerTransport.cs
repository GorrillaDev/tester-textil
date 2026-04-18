namespace AcuratexControlApp.Application.Abstractions;

public interface IControllerTransport : IDisposable
{
    bool IsConnected { get; }
    event Action<string>? LineReceived;

    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync();
    Task SendLineAsync(string line, CancellationToken cancellationToken);
}
