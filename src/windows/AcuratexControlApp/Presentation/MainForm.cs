using System.ComponentModel;
using System.Globalization;
using AcuratexControlApp.Application.Abstractions;
using AcuratexControlApp.Application.Models;
using AcuratexControlApp.Application.Services;
using AcuratexControlApp.Infrastructure.Discovery;
using AcuratexControlApp.Infrastructure.Factories;
using AcuratexControlApp.Shared.Configuration;
using AcuratexControlApp.Shared.Constants;
using AcuratexControlApp.Shared.Models;

namespace AcuratexControlApp.Presentation;

public partial class MainForm : Form
{
    private readonly IDeviceControlService _deviceControlService;
    private readonly bool _ownsDeviceControlService;

    public MainForm()
        : this(CreateDefaultDeviceControlService(), ownsDeviceControlService: true)
    {
    }

    internal MainForm(IDeviceControlService deviceControlService, bool ownsDeviceControlService = false)
    {
        _deviceControlService = deviceControlService ?? throw new ArgumentNullException(nameof(deviceControlService));
        _ownsDeviceControlService = ownsDeviceControlService;

        InitializeComponent();

        if (IsInDesignMode()) {
            return;
        }

        _deviceControlService.LineReceived += OnLineReceived;

        cmbMode.SelectedIndex = 0;
        txtBaud.Text = AcuratexUsbConstants.InterfaceGuidString;
        txtHost.Text = AppDefaults.DefaultWifiHost;
        txtPort.Text = AppDefaults.DefaultWifiPort.ToString(CultureInfo.InvariantCulture);
        txtCommand.Text = AppDefaults.DefaultCommand;

        LoadUsbDevices();
        UpdateConnectionModeSections();
        UpdateUiState(connected: false);
    }

    private static IDeviceControlService CreateDefaultDeviceControlService()
    {
        return new DeviceControlService(
            new ConnectionController(new ControllerTransportFactory()),
            new WinUsbDeviceDiscoveryService());
    }

    private static bool IsInDesignMode()
    {
        return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
    }

    private void btnRefreshPorts_Click(object sender, EventArgs e)
    {
        LoadUsbDevices();
    }

    private void cmbMode_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateConnectionModeSections();
    }

    private async void btnConnect_Click(object sender, EventArgs e)
    {
        btnConnect.Enabled = false;

        try {
            if (GetSelectedMode() == ConnectionMode.Usb) {
                LoadUsbDevices();
            }

            await _deviceControlService.ConnectAsync(CreateConnectionRequestFromUi(), CancellationToken.None);
            AppendLog($"Conectado por {GetSelectedModeLabel()}.");
            UpdateUiState(true);
        } catch (Exception ex) {
            AppendLog($"ERROR connect: {ex.Message}");
            UpdateUiState(false);
        } finally {
            if (!_deviceControlService.IsConnected) {
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
        await SendCommandAsync(txtCommand.Text.Trim());
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

    private string GetSelectedModeLabel()
    {
        return GetSelectedMode() == ConnectionMode.Usb ? "USB" : "WiFi";
    }

    private void UpdateConnectionModeSections()
    {
        bool usbModeSelected = GetSelectedMode() == ConnectionMode.Usb;
        grpUsb.Enabled = usbModeSelected;
        grpWifi.Enabled = !usbModeSelected;
    }

    private void LoadUsbDevices()
    {
        string? selectedDevicePath = (cmbPorts.SelectedItem as UsbVendorDeviceInfo)?.DevicePath;
        IReadOnlyList<UsbVendorDeviceInfo> devices = _deviceControlService.GetUsbDevices();

        cmbPorts.BeginUpdate();
        try {
            cmbPorts.Items.Clear();
            foreach (UsbVendorDeviceInfo device in devices) {
                cmbPorts.Items.Add(device);
            }
        } finally {
            cmbPorts.EndUpdate();
        }

        UsbVendorDeviceInfo? selectedDevice = devices.FirstOrDefault(
            device => string.Equals(device.DevicePath, selectedDevicePath, StringComparison.OrdinalIgnoreCase));

        if (selectedDevice != null) {
            cmbPorts.SelectedItem = selectedDevice;
        } else if (cmbPorts.Items.Count > 0) {
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
        if (!_deviceControlService.IsConnected) {
            UpdateUiState(false);
            return;
        }

        try {
            await _deviceControlService.DisconnectAsync();
            AppendLog("Conexion cerrada.");
        } catch (Exception ex) {
            AppendLog($"ERROR disconnect: {ex.Message}");
        } finally {
            UpdateUiState(false);
        }
    }

    private async Task HandleTransportFaultAsync()
    {
        if (_deviceControlService.IsConnected) {
            return;
        }

        AppendLog("La conexion se cerro. Reenumerando USB.");
        await DisconnectTransportAsync();

        if (GetSelectedMode() == ConnectionMode.Usb) {
            LoadUsbDevices();
        }
    }

    private async Task SendPresetAsync(string command)
    {
        txtCommand.Text = command;
        await SendCommandAsync(command);
    }

    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        await DisconnectTransportAsync();
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (!IsInDesignMode()) {
            _deviceControlService.LineReceived -= OnLineReceived;

            if (_ownsDeviceControlService) {
                _deviceControlService.Dispose();
            }
        }

        base.OnFormClosed(e);
    }

    private ConnectionRequest CreateConnectionRequestFromUi()
    {
        return GetSelectedMode() switch {
            ConnectionMode.Usb => new UsbConnectionRequest(GetSelectedUsbDevice()),
            ConnectionMode.Wifi => new WifiConnectionRequest(ParseHostFromUi(), ParsePortFromUi()),
            _ => throw new NotSupportedException("Modo de conexion no soportado."),
        };
    }

    private UsbVendorDeviceInfo GetSelectedUsbDevice()
    {
        if (cmbPorts.SelectedItem is UsbVendorDeviceInfo device) {
            return device;
        }

        throw new InvalidOperationException("Selecciona un dispositivo USB Acuratex.");
    }

    private string ParseHostFromUi()
    {
        string host = txtHost.Text.Trim();
        if (string.IsNullOrWhiteSpace(host)) {
            throw new InvalidOperationException("Host invalido.");
        }

        return host;
    }

    private int ParsePortFromUi()
    {
        if (!int.TryParse(txtPort.Text.Trim(), out int tcpPort) || tcpPort <= 0) {
            throw new InvalidOperationException("Puerto TCP invalido.");
        }

        return tcpPort;
    }

    private async Task SendCommandAsync(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) {
            return;
        }

        if (!_deviceControlService.IsConnected) {
            AppendLog("No hay conexion activa.");
            return;
        }

        try {
            await _deviceControlService.SendLineAsync(line, CancellationToken.None);
            AppendLog($">> {line}");
        } catch (Exception ex) {
            AppendLog($"ERROR send: {ex.Message}");
            await HandleTransportFaultAsync();
        }
    }
}
