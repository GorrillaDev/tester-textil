using System.Net.Sockets;
using System.Text;

namespace AcuratexControlApp;

public sealed class TcpControllerTransport : IControllerTransport
{
    private readonly string _host;
    private readonly int _port;

    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource? _readLoopCts;
    private Task? _readLoopTask;

    public TcpControllerTransport(string host, int port)
    {
        _host = host;
        _port = port;
    }

    public bool IsConnected => _client?.Connected == true;

    public event Action<string>? LineReceived;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsConnected) {
            return;
        }

        _client = new TcpClient();
        await _client.ConnectAsync(_host, _port, cancellationToken).ConfigureAwait(false);

        NetworkStream stream = _client.GetStream();
        _reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
        _writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true)
        {
            AutoFlush = true,
            NewLine = "\n",
        };

        _readLoopCts = new CancellationTokenSource();
        _readLoopTask = Task.Run(() => ReadLoop(_readLoopCts.Token), CancellationToken.None);
    }

    public async Task DisconnectAsync()
    {
        if (_readLoopCts != null) {
            _readLoopCts.Cancel();
        }

        if (_readLoopTask != null) {
            try {
                await _readLoopTask.ConfigureAwait(false);
            } catch (OperationCanceledException) {
            }
        }

        Cleanup();
    }

    public async Task SendLineAsync(string line, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_writer == null) {
            throw new InvalidOperationException("La conexion TCP no esta conectada.");
        }

        await _writer.WriteLineAsync(line).ConfigureAwait(false);
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }

    private async Task ReadLoop(CancellationToken cancellationToken)
    {
        if (_reader == null) {
            return;
        }

        while (!cancellationToken.IsCancellationRequested) {
            string? line;

            try {
                line = await _reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            } catch (OperationCanceledException) {
                break;
            } catch (IOException) {
                break;
            } catch (ObjectDisposedException) {
                break;
            }

            if (line == null) {
                break;
            }

            if (!string.IsNullOrWhiteSpace(line)) {
                LineReceived?.Invoke(line.Trim());
            }
        }
    }

    private void Cleanup()
    {
        _reader?.Dispose();
        _reader = null;

        _writer?.Dispose();
        _writer = null;

        _client?.Dispose();
        _client = null;

        _readLoopTask = null;

        if (_readLoopCts != null) {
            _readLoopCts.Dispose();
            _readLoopCts = null;
        }
    }
}
