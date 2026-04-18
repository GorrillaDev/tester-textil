namespace AcuratexControlApp.Presentation;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;
    private ComboBox cmbMode;
    private GroupBox grpUsb;
    private Button btnRefreshPorts;
    private TextBox txtBaud;
    private Label lblBaud;
    private ComboBox cmbPorts;
    private Label lblPort;
    private GroupBox grpWifi;
    private Label lblWifiHint;
    private TextBox txtPort;
    private Label lblTcpPort;
    private TextBox txtHost;
    private Label lblHost;
    private Button btnConnect;
    private Button btnDisconnect;
    private Label lblStatus;
    private Label lblStatusValue;
    private TextBox txtCommand;
    private Button btnSend;
    private TextBox txtLog;
    private Label lblCommand;
    private Label lblLog;
    private Button btnSendStart;
    private Button btnSendStop;
    private Button btnSendTest;
    private Label lblMode;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.cmbMode = new System.Windows.Forms.ComboBox();
        this.grpUsb = new System.Windows.Forms.GroupBox();
        this.btnRefreshPorts = new System.Windows.Forms.Button();
        this.txtBaud = new System.Windows.Forms.TextBox();
        this.lblBaud = new System.Windows.Forms.Label();
        this.cmbPorts = new System.Windows.Forms.ComboBox();
        this.lblPort = new System.Windows.Forms.Label();
        this.grpWifi = new System.Windows.Forms.GroupBox();
        this.lblWifiHint = new System.Windows.Forms.Label();
        this.txtPort = new System.Windows.Forms.TextBox();
        this.lblTcpPort = new System.Windows.Forms.Label();
        this.txtHost = new System.Windows.Forms.TextBox();
        this.lblHost = new System.Windows.Forms.Label();
        this.btnConnect = new System.Windows.Forms.Button();
        this.btnDisconnect = new System.Windows.Forms.Button();
        this.lblStatus = new System.Windows.Forms.Label();
        this.lblStatusValue = new System.Windows.Forms.Label();
        this.txtCommand = new System.Windows.Forms.TextBox();
        this.btnSend = new System.Windows.Forms.Button();
        this.txtLog = new System.Windows.Forms.TextBox();
        this.lblCommand = new System.Windows.Forms.Label();
        this.lblLog = new System.Windows.Forms.Label();
        this.btnSendStart = new System.Windows.Forms.Button();
        this.btnSendStop = new System.Windows.Forms.Button();
        this.btnSendTest = new System.Windows.Forms.Button();
        this.lblMode = new System.Windows.Forms.Label();
        this.grpUsb.SuspendLayout();
        this.grpWifi.SuspendLayout();
        this.SuspendLayout();
        // 
        // cmbMode
        // 
        this.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbMode.FormattingEnabled = true;
        this.cmbMode.Items.AddRange(new object[] {
            "USB",
            "WiFi"});
        this.cmbMode.Location = new System.Drawing.Point(108, 18);
        this.cmbMode.Name = "cmbMode";
        this.cmbMode.Size = new System.Drawing.Size(141, 23);
        this.cmbMode.TabIndex = 0;
        this.cmbMode.SelectedIndexChanged += new System.EventHandler(this.cmbMode_SelectedIndexChanged);
        // 
        // grpUsb
        // 
        this.grpUsb.Controls.Add(this.btnRefreshPorts);
        this.grpUsb.Controls.Add(this.txtBaud);
        this.grpUsb.Controls.Add(this.lblBaud);
        this.grpUsb.Controls.Add(this.cmbPorts);
        this.grpUsb.Controls.Add(this.lblPort);
        this.grpUsb.Location = new System.Drawing.Point(12, 56);
        this.grpUsb.Name = "grpUsb";
        this.grpUsb.Size = new System.Drawing.Size(340, 108);
        this.grpUsb.TabIndex = 1;
        this.grpUsb.TabStop = false;
        this.grpUsb.Text = "USB nativo (WinUSB)";
        // 
        // btnRefreshPorts
        // 
        this.btnRefreshPorts.Location = new System.Drawing.Point(236, 28);
        this.btnRefreshPorts.Name = "btnRefreshPorts";
        this.btnRefreshPorts.Size = new System.Drawing.Size(88, 23);
        this.btnRefreshPorts.TabIndex = 4;
        this.btnRefreshPorts.Text = "Buscar";
        this.btnRefreshPorts.UseVisualStyleBackColor = true;
        this.btnRefreshPorts.Click += new System.EventHandler(this.btnRefreshPorts_Click);
        // 
        // txtBaud
        // 
        this.txtBaud.Location = new System.Drawing.Point(88, 66);
        this.txtBaud.Name = "txtBaud";
        this.txtBaud.ReadOnly = true;
        this.txtBaud.Size = new System.Drawing.Size(236, 23);
        this.txtBaud.TabIndex = 3;
        // 
        // lblBaud
        // 
        this.lblBaud.AutoSize = true;
        this.lblBaud.Location = new System.Drawing.Point(15, 69);
        this.lblBaud.Name = "lblBaud";
        this.lblBaud.Size = new System.Drawing.Size(36, 15);
        this.lblBaud.TabIndex = 2;
        this.lblBaud.Text = "GUID:";
        // 
        // cmbPorts
        // 
        this.cmbPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbPorts.FormattingEnabled = true;
        this.cmbPorts.Location = new System.Drawing.Point(88, 28);
        this.cmbPorts.Name = "cmbPorts";
        this.cmbPorts.Size = new System.Drawing.Size(136, 23);
        this.cmbPorts.TabIndex = 1;
        // 
        // lblPort
        // 
        this.lblPort.AutoSize = true;
        this.lblPort.Location = new System.Drawing.Point(15, 31);
        this.lblPort.Name = "lblPort";
        this.lblPort.Size = new System.Drawing.Size(67, 15);
        this.lblPort.TabIndex = 0;
        this.lblPort.Text = "Dispositivo";
        // 
        // grpWifi
        // 
        this.grpWifi.Controls.Add(this.lblWifiHint);
        this.grpWifi.Controls.Add(this.txtPort);
        this.grpWifi.Controls.Add(this.lblTcpPort);
        this.grpWifi.Controls.Add(this.txtHost);
        this.grpWifi.Controls.Add(this.lblHost);
        this.grpWifi.Location = new System.Drawing.Point(371, 56);
        this.grpWifi.Name = "grpWifi";
        this.grpWifi.Size = new System.Drawing.Size(380, 108);
        this.grpWifi.TabIndex = 2;
        this.grpWifi.TabStop = false;
        this.grpWifi.Text = "Conexion WiFi";
        // 
        // lblWifiHint
        // 
        this.lblWifiHint.AutoSize = true;
        this.lblWifiHint.ForeColor = System.Drawing.SystemColors.GrayText;
        this.lblWifiHint.Location = new System.Drawing.Point(15, 79);
        this.lblWifiHint.Name = "lblWifiHint";
        this.lblWifiHint.Size = new System.Drawing.Size(326, 15);
        this.lblWifiHint.TabIndex = 4;
        this.lblWifiHint.Text = "Usa esto cuando el ESP este en la misma red o hotspot fijo.";
        // 
        // txtPort
        // 
        this.txtPort.Location = new System.Drawing.Point(266, 28);
        this.txtPort.Name = "txtPort";
        this.txtPort.Size = new System.Drawing.Size(81, 23);
        this.txtPort.TabIndex = 3;
        // 
        // lblTcpPort
        // 
        this.lblTcpPort.AutoSize = true;
        this.lblTcpPort.Location = new System.Drawing.Point(226, 31);
        this.lblTcpPort.Name = "lblTcpPort";
        this.lblTcpPort.Size = new System.Drawing.Size(34, 15);
        this.lblTcpPort.TabIndex = 2;
        this.lblTcpPort.Text = "Port:";
        // 
        // txtHost
        // 
        this.txtHost.Location = new System.Drawing.Point(58, 28);
        this.txtHost.Name = "txtHost";
        this.txtHost.Size = new System.Drawing.Size(150, 23);
        this.txtHost.TabIndex = 1;
        // 
        // lblHost
        // 
        this.lblHost.AutoSize = true;
        this.lblHost.Location = new System.Drawing.Point(15, 31);
        this.lblHost.Name = "lblHost";
        this.lblHost.Size = new System.Drawing.Size(34, 15);
        this.lblHost.TabIndex = 0;
        this.lblHost.Text = "Host:";
        // 
        // btnConnect
        // 
        this.btnConnect.Location = new System.Drawing.Point(12, 182);
        this.btnConnect.Name = "btnConnect";
        this.btnConnect.Size = new System.Drawing.Size(116, 30);
        this.btnConnect.TabIndex = 3;
        this.btnConnect.Text = "Conectar";
        this.btnConnect.UseVisualStyleBackColor = true;
        this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
        // 
        // btnDisconnect
        // 
        this.btnDisconnect.Location = new System.Drawing.Point(144, 182);
        this.btnDisconnect.Name = "btnDisconnect";
        this.btnDisconnect.Size = new System.Drawing.Size(116, 30);
        this.btnDisconnect.TabIndex = 4;
        this.btnDisconnect.Text = "Desconectar";
        this.btnDisconnect.UseVisualStyleBackColor = true;
        this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
        // 
        // lblStatus
        // 
        this.lblStatus.AutoSize = true;
        this.lblStatus.Location = new System.Drawing.Point(286, 190);
        this.lblStatus.Name = "lblStatus";
        this.lblStatus.Size = new System.Drawing.Size(45, 15);
        this.lblStatus.TabIndex = 5;
        this.lblStatus.Text = "Estado:";
        // 
        // lblStatusValue
        // 
        this.lblStatusValue.AutoSize = true;
        this.lblStatusValue.Location = new System.Drawing.Point(337, 190);
        this.lblStatusValue.Name = "lblStatusValue";
        this.lblStatusValue.Size = new System.Drawing.Size(82, 15);
        this.lblStatusValue.TabIndex = 6;
        this.lblStatusValue.Text = "Desconectado";
        // 
        // txtCommand
        // 
        this.txtCommand.Location = new System.Drawing.Point(108, 232);
        this.txtCommand.Name = "txtCommand";
        this.txtCommand.Size = new System.Drawing.Size(461, 23);
        this.txtCommand.TabIndex = 7;
        // 
        // btnSend
        // 
        this.btnSend.Location = new System.Drawing.Point(586, 231);
        this.btnSend.Name = "btnSend";
        this.btnSend.Size = new System.Drawing.Size(165, 25);
        this.btnSend.TabIndex = 8;
        this.btnSend.Text = "Enviar linea";
        this.btnSend.UseVisualStyleBackColor = true;
        this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
        // 
        // txtLog
        // 
        this.txtLog.Location = new System.Drawing.Point(12, 312);
        this.txtLog.Multiline = true;
        this.txtLog.Name = "txtLog";
        this.txtLog.ReadOnly = true;
        this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        this.txtLog.Size = new System.Drawing.Size(739, 196);
        this.txtLog.TabIndex = 9;
        // 
        // lblCommand
        // 
        this.lblCommand.AutoSize = true;
        this.lblCommand.Location = new System.Drawing.Point(12, 235);
        this.lblCommand.Name = "lblCommand";
        this.lblCommand.Size = new System.Drawing.Size(90, 15);
        this.lblCommand.TabIndex = 10;
        this.lblCommand.Text = "Comando CAN:";
        // 
        // lblLog
        // 
        this.lblLog.AutoSize = true;
        this.lblLog.Location = new System.Drawing.Point(12, 291);
        this.lblLog.Name = "lblLog";
        this.lblLog.Size = new System.Drawing.Size(28, 15);
        this.lblLog.TabIndex = 11;
        this.lblLog.Text = "Log";
        // 
        // btnSendStart
        // 
        this.btnSendStart.Location = new System.Drawing.Point(12, 271);
        this.btnSendStart.Name = "btnSendStart";
        this.btnSendStart.Size = new System.Drawing.Size(110, 25);
        this.btnSendStart.TabIndex = 12;
        this.btnSendStart.Text = "Enviar start";
        this.btnSendStart.UseVisualStyleBackColor = true;
        this.btnSendStart.Click += new System.EventHandler(this.btnSendStart_Click);
        // 
        // btnSendStop
        // 
        this.btnSendStop.Location = new System.Drawing.Point(138, 271);
        this.btnSendStop.Name = "btnSendStop";
        this.btnSendStop.Size = new System.Drawing.Size(110, 25);
        this.btnSendStop.TabIndex = 13;
        this.btnSendStop.Text = "Enviar stop";
        this.btnSendStop.UseVisualStyleBackColor = true;
        this.btnSendStop.Click += new System.EventHandler(this.btnSendStop_Click);
        // 
        // btnSendTest
        // 
        this.btnSendTest.Location = new System.Drawing.Point(264, 271);
        this.btnSendTest.Name = "btnSendTest";
        this.btnSendTest.Size = new System.Drawing.Size(110, 25);
        this.btnSendTest.TabIndex = 14;
        this.btnSendTest.Text = "Enviar testeo";
        this.btnSendTest.UseVisualStyleBackColor = true;
        this.btnSendTest.Click += new System.EventHandler(this.btnSendTest_Click);
        // 
        // lblMode
        // 
        this.lblMode.AutoSize = true;
        this.lblMode.Location = new System.Drawing.Point(12, 21);
        this.lblMode.Name = "lblMode";
        this.lblMode.Size = new System.Drawing.Size(90, 15);
        this.lblMode.TabIndex = 15;
        this.lblMode.Text = "Tipo conexion:";
        // 
        // MainForm
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(765, 520);
        this.Controls.Add(this.lblMode);
        this.Controls.Add(this.btnSendTest);
        this.Controls.Add(this.btnSendStop);
        this.Controls.Add(this.btnSendStart);
        this.Controls.Add(this.lblLog);
        this.Controls.Add(this.lblCommand);
        this.Controls.Add(this.txtLog);
        this.Controls.Add(this.btnSend);
        this.Controls.Add(this.txtCommand);
        this.Controls.Add(this.lblStatusValue);
        this.Controls.Add(this.lblStatus);
        this.Controls.Add(this.btnDisconnect);
        this.Controls.Add(this.btnConnect);
        this.Controls.Add(this.grpWifi);
        this.Controls.Add(this.grpUsb);
        this.Controls.Add(this.cmbMode);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.Name = "MainForm";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Acuratex Control App";
        this.grpUsb.ResumeLayout(false);
        this.grpUsb.PerformLayout();
        this.grpWifi.ResumeLayout(false);
        this.grpWifi.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion
}
