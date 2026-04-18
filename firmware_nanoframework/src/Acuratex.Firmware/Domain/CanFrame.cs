namespace Acuratex.Firmware.Domain;

public sealed class CanFrame
{
    public CanFrame(uint id, byte[] data)
    {
        Id = id;
        Data = data;
    }

    public uint Id { get; }
    public byte[] Data { get; }

    public int DataLength => Data == null ? 0 : Data.Length;
    public string IdHex => Id.ToString("X3");
}
