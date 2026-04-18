using System;
using Acuratex.Firmware.Domain;

namespace Acuratex.Firmware.Infrastructure;

public sealed class ConsoleLogger : ILogger
{
    public void Info(string message) => Console.WriteLine("[I] " + message);
    public void Warn(string message) => Console.WriteLine("[W] " + message);
    public void Error(string message) => Console.WriteLine("[E] " + message);
}

public sealed class UartCommandChannel : ICommandChannel
{
    public string ReceiveLine()
    {
        return Console.ReadLine();
    }

    public void SendLine(string line)
    {
        Console.WriteLine(line);
    }
}

public sealed class CanService : ICanService
{
    private int _activeBus = 1;

    public bool SelectBus(int bus)
    {
        if (bus != 1 && bus != 2)
        {
            return false;
        }

        _activeBus = bus;
        return true;
    }

    public bool Send(CanFrame frame)
    {
        // Placeholder para driver CAN real en nanoFramework.
        return frame != null;
    }

    public string GetActiveBusName() => _activeBus == 2 ? "CAN2" : "CAN1";
}

public sealed class HeadService : IHeadService
{
    public void Test()
    {
        // Placeholder para comandos reales de cabezal.
    }
}

public sealed class MotorService : IMotorService
{
    public void Start()
    {
        // Placeholder para start real.
    }

    public void Stop()
    {
        // Placeholder para stop real.
    }
}

public sealed class SystemStatusService : ISystemStatusService
{
    private readonly ICanService _canService;

    public SystemStatusService(ICanService canService)
    {
        _canService = canService;
    }

    public string GetStatusLine()
    {
        return "STATUS usb=todo wifi=todo ip=todo tcp_port=3333 can=" + _canService.GetActiveBusName() + " ssid=ACURATEX_NET";
    }
}
