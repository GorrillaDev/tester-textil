using System;
using Acuratex.Firmware.Domain;

namespace Acuratex.Firmware.Core;

public sealed class CommandRouter : ICommandRouter
{
    private readonly ICanService _canService;
    private readonly IHeadService _headService;
    private readonly IMotorService _motorService;
    private readonly ISystemStatusService _statusService;

    public CommandRouter(
        ICanService canService,
        IHeadService headService,
        IMotorService motorService,
        ISystemStatusService statusService)
    {
        _canService = canService;
        _headService = headService;
        _motorService = motorService;
        _statusService = statusService;
    }

    public string Route(string incoming)
    {
        if (incoming == null)
        {
            return "ERR null";
        }

        string line = incoming.Trim();
        if (line.Length == 0)
        {
            return string.Empty;
        }

        string lower = line.ToLower();

        if (lower == "ping" || lower == "hello")
        {
            return "PONG";
        }

        if (lower == "status")
        {
            return _statusService.GetStatusLine();
        }

        if (lower == "can1")
        {
            return _canService.SelectBus(1) ? "OK CAN1" : "ERR no se pudo activar CAN1";
        }

        if (lower == "can2")
        {
            return _canService.SelectBus(2) ? "OK CAN2" : "ERR no se pudo activar CAN2";
        }

        if (lower == "start")
        {
            _motorService.Start();
            return "ACK start";
        }

        if (lower == "stop")
        {
            _motorService.Stop();
            return "ACK stop";
        }

        if (lower == "testeo")
        {
            _headService.Test();
            return "ACK testeo";
        }

        if (lower.StartsWith("send "))
        {
            return HandleSend(line.Substring(5).Trim());
        }

        return HandleSend(line);
    }

    private string HandleSend(string payload)
    {
        CanFrame frame = CanFrameParser.Parse(payload);
        if (frame == null)
        {
            return "ERR frame invalido";
        }

        bool ok = _canService.Send(frame);
        if (!ok)
        {
            return "ERR can_send";
        }

        return "TX_OK bus=" + _canService.GetActiveBusName() + " id=0x" + frame.IdHex + " dlc=" + frame.DataLength;
    }
}
