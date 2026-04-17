using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace AcuratexControlApp;

public sealed class WinUsbControllerTransport : IControllerTransport
{
    private readonly string _devicePath;
    private readonly StringBuilder _lineBuilder = new();

    private SafeFileHandle? _deviceHandle;
    private nint _winUsbHandle;
    private byte _readPipeId;
    private byte _writePipeId;
    private CancellationTokenSource? _readLoopCts;
    private Task? _readLoopTask;

    public WinUsbControllerTransport(string devicePath)
    {
        _devicePath = devicePath;
    }

    public bool IsConnected =>
        _deviceHandle is { IsInvalid: false, IsClosed: false } &&
        _winUsbHandle != nint.Zero;

    public event Action<string>? LineReceived;

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsConnected) {
            return Task.CompletedTask;
        }

        _deviceHandle = WinUsbNative.CreateFileW(
            _devicePath,
            WinUsbNative.GenericRead | WinUsbNative.GenericWrite,
            WinUsbNative.FileShareRead | WinUsbNative.FileShareWrite,
            IntPtr.Zero,
            WinUsbNative.OpenExisting,
            WinUsbNative.FileAttributeNormal | WinUsbNative.FileFlagOverlapped,
            IntPtr.Zero);

        if (_deviceHandle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo abrir el dispositivo WinUSB.");
        }

        if (!WinUsbNative.WinUsb_Initialize(_deviceHandle, out _winUsbHandle)) {
            int error = Marshal.GetLastWin32Error();
            _deviceHandle.Dispose();
            _deviceHandle = null;
            throw new Win32Exception(error, $"No se pudo inicializar WinUSB (error {error}).");
        }

        DiscoverBulkPipes();
        ConfigurePipeTimeout(_readPipeId, 200);
        ConfigurePipeTimeout(_writePipeId, 200);

        _readLoopCts = new CancellationTokenSource();
        _readLoopTask = Task.Run(() => ReadLoop(_readLoopCts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task DisconnectAsync()
    {
        _readLoopCts?.Cancel();
        AbortOpenPipes();
        CloseNativeHandles();

        if (_readLoopTask != null) {
            try {
                await _readLoopTask.ConfigureAwait(false);
            } catch (OperationCanceledException) {
            }
        }

        _readLoopTask = null;

        if (_readLoopCts != null) {
            _readLoopCts.Dispose();
            _readLoopCts = null;
        }
    }

    public Task SendLineAsync(string line, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsConnected) {
            throw new InvalidOperationException("El dispositivo USB no esta conectado.");
        }

        byte[] payload = Encoding.ASCII.GetBytes(line + "\n");
        if (!WinUsbNative.WinUsb_WritePipe(_winUsbHandle, _writePipeId, payload, payload.Length, out int transferred, IntPtr.Zero)) {
            int error = Marshal.GetLastWin32Error();
            if (error == WinUsbNative.ErrorOperationAborted ||
                error == WinUsbNative.ErrorDeviceNotConnected ||
                error == WinUsbNative.ErrorInvalidHandle ||
                error == WinUsbNative.ErrorGenFailure) {
                CloseNativeHandles();
            }

            throw new Win32Exception(error, "No se pudo escribir al dispositivo USB.");
        }

        if (transferred != payload.Length) {
            throw new IOException("La escritura USB fue parcial.");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }

    private void DiscoverBulkPipes()
    {
        if (!WinUsbNative.WinUsb_QueryInterfaceSettings(_winUsbHandle, 0, out WinUsbNative.UsbInterfaceDescriptor descriptor)) {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo consultar la interfaz USB.");
        }

        byte bulkIn = 0;
        byte bulkOut = 0;

        for (byte index = 0; index < descriptor.NumEndpoints; index++) {
            if (!WinUsbNative.WinUsb_QueryPipe(_winUsbHandle, 0, index, out WinUsbNative.WinUsbPipeInformation pipe)) {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo consultar los endpoints USB.");
            }

            if (pipe.PipeType != WinUsbNative.UsbdPipeType.Bulk) {
                continue;
            }

            if ((pipe.PipeId & 0x80) != 0) {
                bulkIn = pipe.PipeId;
            } else {
                bulkOut = pipe.PipeId;
            }
        }

        if (bulkIn == 0 || bulkOut == 0) {
            throw new InvalidOperationException("El dispositivo USB no expone endpoints bulk IN/OUT validos.");
        }

        _readPipeId = bulkIn;
        _writePipeId = bulkOut;
    }

    private void ConfigurePipeTimeout(byte pipeId, uint timeoutMs)
    {
        if (_winUsbHandle == nint.Zero || pipeId == 0) {
            return;
        }

        WinUsbNative.WinUsb_SetPipePolicy(
            _winUsbHandle,
            pipeId,
            WinUsbNative.PipeTransferTimeoutPolicy,
            (uint)sizeof(uint),
            ref timeoutMs);
    }

    private void AbortOpenPipes()
    {
        if (_winUsbHandle == nint.Zero) {
            return;
        }

        if (_readPipeId != 0) {
            WinUsbNative.WinUsb_AbortPipe(_winUsbHandle, _readPipeId);
        }

        if (_writePipeId != 0) {
            WinUsbNative.WinUsb_AbortPipe(_winUsbHandle, _writePipeId);
        }
    }

    private void CloseNativeHandles()
    {
        if (_winUsbHandle != nint.Zero) {
            WinUsbNative.WinUsb_Free(_winUsbHandle);
            _winUsbHandle = nint.Zero;
        }

        if (_deviceHandle != null) {
            _deviceHandle.Dispose();
            _deviceHandle = null;
        }
    }

    private void ReadLoop(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[512];

        try {
            while (!cancellationToken.IsCancellationRequested) {
                if (_winUsbHandle == nint.Zero) {
                    break;
                }

                bool ok = WinUsbNative.WinUsb_ReadPipe(_winUsbHandle, _readPipeId, buffer, buffer.Length, out int transferred, IntPtr.Zero);
                if (!ok) {
                    int error = Marshal.GetLastWin32Error();
                    if (error == WinUsbNative.ErrorSemTimeout) {
                        continue;
                    }

                    if (error == WinUsbNative.ErrorOperationAborted ||
                        error == WinUsbNative.ErrorDeviceNotConnected ||
                        error == WinUsbNative.ErrorInvalidHandle ||
                        error == WinUsbNative.ErrorGenFailure) {
                        break;
                    }

                    throw new Win32Exception(error, "Error leyendo desde WinUSB.");
                }

                if (transferred <= 0) {
                    continue;
                }

                ProcessIncomingBytes(buffer, transferred);
            }
        } finally {
            CloseNativeHandles();
        }
    }

    private void ProcessIncomingBytes(byte[] buffer, int transferred)
    {
        for (int i = 0; i < transferred; i++) {
            char c = (char)buffer[i];

            if (c == '\r') {
                continue;
            }

            if (c == '\n') {
                if (_lineBuilder.Length > 0) {
                    string line = _lineBuilder.ToString().Trim();
                    _lineBuilder.Clear();

                    if (!string.IsNullOrWhiteSpace(line)) {
                        LineReceived?.Invoke(line);
                    }
                }
                continue;
            }

            _lineBuilder.Append(c);
        }
    }
}
