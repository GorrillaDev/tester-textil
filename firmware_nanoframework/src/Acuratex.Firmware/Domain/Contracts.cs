namespace Acuratex.Firmware.Domain;

public interface ICommandRouter
{
    string Route(string incoming);
}

public interface ICommandChannel
{
    string ReceiveLine();
    void SendLine(string line);
}

public interface ICanService
{
    bool SelectBus(int bus);
    bool Send(CanFrame frame);
    string GetActiveBusName();
}

public interface IHeadService
{
    void Test();
}

public interface IMotorService
{
    void Start();
    void Stop();
}

public interface ISystemStatusService
{
    string GetStatusLine();
}

public interface ILogger
{
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}
