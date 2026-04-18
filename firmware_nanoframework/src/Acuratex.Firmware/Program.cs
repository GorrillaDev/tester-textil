using Acuratex.Firmware.Composition;

namespace Acuratex.Firmware;

public static class Program
{
    public static void Main()
    {
        FirmwareApp app = AppComposer.Build();
        app.Run();
    }
}
