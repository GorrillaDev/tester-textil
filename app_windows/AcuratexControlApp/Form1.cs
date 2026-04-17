namespace AcuratexControlApp;

public partial class Form1 : Form
{
    private IControllerTransport? _transport;

    public Form1()
    {
        InitializeComponent();
        cmbMode.SelectedIndex = 0;
        txtBaud.Text = AcuratexUsbConstants.InterfaceGuidString;
        txtHost.Text = "192.168.137.2";
        txtPort.Text = "3333";
        txtCommand.Text = "320 07";
        RefreshUsbDevices();
        UpdateUiState(false);
    }

    private void btnRefreshPorts_Click(object sender, EventArgs e)
    {
        RefreshUsbDevices();
    }

    private void cmbMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool usb = GetSelectedMode() == ConnectionMode.Usb;
        grpUsb.Enabled = usb;
        grpWifi.Enabled = !usb;
    }

    private async void btnConnect_Click(object sender, EventArgs e)
    {
        btnConnect.Enabled = false;

        try {
            if (GetSelectedMode() == ConnectionMode.Usb) {
                RefreshUsbDevices();
            }

            _transport?.Dispose();
            _transport = CreateTransportFromUi();
            _transport.LineReceived += OnLineReceived;

            await _transport.ConnectAsync(CancellationToken.None);
            AppendLog($"Conectado por {GetSelectedMode()}.");
            UpdateUiState(true);
        } catch (Exception ex) {
            AppendLog($"ERROR connect: {ex.Message}");
            UpdateUiState(false);
        } finally {
            if (_transport == null || !_transport.IsConnected) {
                btnConnect.Enabled = true;
            }
        }
    }

    private async void btnDisconnect_Click(object sender, EventArgs e)
    {
        await DisconnectTransportAsync();
    }

    private async void btnSend_Click(object sender, EventArgs e)
    {
        string line = txtCommand.Text.Trim();
        if (string.IsNullOrWhiteSpace(line)) {
            return;
        }

        if (_transport == null || !_transport.IsConnected) {
            AppendLog("No hay conexion activa.");
            return;
        }

        try {
            await _transport.SendLineAsync(line, CancellationToken.None);
            AppendLog($">> {line}");
        } catch (Exception ex) {
            AppendLog($"ERROR send: {ex.Message}");
            await HandleTransportFaultAsync();
        }
    }

    private async void btnSendStart_Click(object sender, EventArgs e)
    {
        await SendPresetAsync("start");
    }

    private async void btnSendStop_Click(object sender, EventArgs e)
    {
        await SendPresetAsync("stop");
    }

    private async void btnSendTest_Click(object sender, EventArgs e)
    {
        await SendPresetAsync("testeo");
    }

    private void OnLineReceived(string line)
    {
        if (InvokeRequired) {
            BeginInvoke(() => AppendLog($"<< {line}"));
            return;
        }

        AppendLog($"<< {line}");
    }

    private ConnectionMode GetSelectedMode()
    {
        return cmbMode.SelectedIndex == 0 ? ConnectionMode.Usb : ConnectionMode.Wifi;
    }

    private IControllerTransport CreateTransportFromUi()
    {
        if (GetSelectedMode() == ConnectionMode.Usb) {
            if (cmbPorts.SelectedItem is not UsbVendorDeviceInfo device) {
                throw new InvalidOperationException("Selecciona un dispositivo USB Acuratex.");
            }

            return new WinUsbControllerTransport(device.DevicePath);
        }

        string host = txtHost.Text.Trim();
        if (string.IsNullOrWhiteSpace(host)) {
            throw new InvalidOperationException("Host invalido.");
        }

        if (!int.TryParse(txtPort.Text.Trim(), out int tcpPort) || tcpPort <= 0) {
            throw new InvalidOperationException("Puerto TCP invalido.");
        }

        return new TcpControllerTransport(host, tcpPort);
    }

    private void RefreshUsbDevices()
    {
        IReadOnlyList<UsbVendorDeviceInfo> devices = WinUsbDeviceEnumerator
            .Enumerate(AcuratexUsbConstants.InterfaceGuid)
            .OrderBy(static x => x.DevicePath, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        cmbPorts.Items.Clear();
        foreach (UsbVendorDeviceInfo device in devices) {
            cmbPorts.Items.Add(device);
        }

        if (cmbPorts.Items.Count > 0) {
            cmbPorts.SelectedIndex = 0;
        }

        AppendLog($"USB WinUSB detectados: {devices.Count}");
    }

    private void AppendLog(string text)
    {
        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
    }

    private void UpdateUiState(bool connected)
    {
        lblStatusValue.Text = connected ? "Conectado" : "Desconectado";
        lblStatusValue.ForeColor = connected ? Color.ForestGreen : Color.Firebrick;

        btnConnect.Enabled = !connected;
        btnDisconnect.Enabled = connected;
        btnSend.Enabled = connected;
        btnSendStart.Enabled = connected;
        btnSendStop.Enabled = connected;
        btnSendTest.Enabled = connected;
    }

    private async Task DisconnectTransportAsync()
    {
        if (_transport == null) {
            UpdateUiState(false);
            return;
        }

        try {
            await _transport.DisconnectAsync();
            AppendLog("Conexion cerrada.");
        } catch (Exception ex) {
            AppendLog($"ERROR disconnect: {ex.Message}");
        } finally {
            _transport.Dispose();
            _transport = null;
            UpdateUiState(false);
        }
    }

    private async Task HandleTransportFaultAsync()
    {
        if (_transport == null) {
            UpdateUiState(false);
            return;
        }

        if (_transport.IsConnected) {
            return;
        }

        AppendLog("La conexion se cerro. Reenumerando USB.");
        await DisconnectTransportAsync();

        if (GetSelectedMode() == ConnectionMode.Usb) {
            RefreshUsbDevices();
        }
    }

    private async Task SendPresetAsync(string command)
    {
        txtCommand.Text = command;
        await Task.Yield();
        btnSend.PerformClick();
    }

    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        await DisconnectTransportAsync();
        base.OnFormClosing(e);
    }
}
