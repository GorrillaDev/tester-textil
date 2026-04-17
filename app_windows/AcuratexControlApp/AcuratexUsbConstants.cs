namespace AcuratexControlApp;

public static class AcuratexUsbConstants
{
    public const ushort VendorId = 0xCAFE;
    public const ushort ProductId = 0x4030;
    public const string InterfaceGuidString = "{D7761D50-5F1B-4D33-95F2-733B0E5F2EED}";

    public static readonly Guid InterfaceGuid = new(InterfaceGuidString);
}
