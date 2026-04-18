using Acuratex.Firmware.Core;
using Acuratex.Firmware.Domain;
using Acuratex.Firmware.Infrastructure;

namespace Acuratex.Firmware.Composition;

public static class AppComposer
{
    public static FirmwareApp Build()
    {
        ILogger logger = new ConsoleLogger();
        ICanService canService = new CanService();
        IHeadService headService = new HeadService();
        IMotorService motorService = new MotorService();
        ISystemStatusService statusService = new SystemStatusService(canService);

        ICommandRouter router = new CommandRouter(canService, headService, motorService, statusService);
        ICommandChannel channel = new UartCommandChannel();

        return new FirmwareApp(router, channel, logger);
    }
}
