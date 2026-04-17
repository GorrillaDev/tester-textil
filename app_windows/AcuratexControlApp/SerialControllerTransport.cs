using System.IO.Ports;

namespace AcuratexControlApp;

public sealed class SerialControllerTransport : IControllerTransport
{
    private readonly string _portName;
    private readonly int _baudRate;
    private SerialPort? _serialPort;
    private CancellationTokenSource? _readLoopCts;
    private Task? _readLoopTask;

    public SerialControllerTransport(string portName, int baudRate)
    {
        _portName = portName;
        _baudRate = baudRate;
    }

    public bool IsConnected => _serialPort?.IsOpen == true;

    public event Action<string>? LineReceived;

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsConnected) {
            return Task.CompletedTask;
        }

        _serialPort = new SerialPort(_portName, _baudRate)
        {
            NewLine = "\n",
            ReadTimeout = 250,
            WriteTimeout = 1000,
            DtrEnable = true,
            RtsEnable = true,
        };
        _serialPort.Open();

        _readLoopCts = new CancellationTokenSource();
        _readLoopTask = Task.Run(() => ReadLoop(_readLoopCts.Token), CancellationToken.None);

        return Task.CompletedTask;
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

    public Task SendLineAsync(string line, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_serialPort == null || !_serialPort.IsOpen) {
            throw new InvalidOperationException("El puerto serie no esta conectado.");
        }

        _serialPort.WriteLine(line);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }

    private void ReadLoop(CancellationToken cancellationToken)
    {
        if (_serialPort == null) {
            return;
        }

        while (!cancellationToken.IsCancellationRequested && _serialPort.IsOpen) {
            try {
                string line = _serialPort.ReadLine();
                if (!string.IsNullOrWhiteSpace(line)) {
                    LineReceived?.Invoke(line.Trim());
                }
            } catch (TimeoutException) {
            } catch (InvalidOperationException) {
                break;
            }
        }
    }

    private void Cleanup()
    {
        if (_serialPort != null) {
            if (_serialPort.IsOpen) {
                _serialPort.Close();
            }

            _serialPort.Dispose();
            _serialPort = null;
        }

        _readLoopTask = null;

        if (_readLoopCts != null) {
            _readLoopCts.Dispose();
            _readLoopCts = null;
        }
    }
}
