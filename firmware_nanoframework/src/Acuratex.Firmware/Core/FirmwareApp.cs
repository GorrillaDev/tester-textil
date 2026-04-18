using Acuratex.Firmware.Domain;

namespace Acuratex.Firmware.Core;

public sealed class FirmwareApp
{
    private readonly ICommandRouter _commandRouter;
    private readonly ICommandChannel _commandChannel;
    private readonly ILogger _logger;

    public FirmwareApp(ICommandRouter commandRouter, ICommandChannel commandChannel, ILogger logger)
    {
        _commandRouter = commandRouter;
        _commandChannel = commandChannel;
        _logger = logger;
    }

    public void Run()
    {
        _logger.Info("ACURATEX FW nanoFramework READY");

        while (true)
        {
            string incoming = _commandChannel.ReceiveLine();
            if (incoming == null)
            {
                continue;
            }

            string response = _commandRouter.Route(incoming);
            if (response != null && response.Length > 0)
            {
                _commandChannel.SendLine(response);
            }
        }
    }
}
