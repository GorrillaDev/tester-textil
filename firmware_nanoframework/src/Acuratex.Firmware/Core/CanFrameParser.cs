using System;
using Acuratex.Firmware.Domain;

namespace Acuratex.Firmware.Core;

public static class CanFrameParser
{
    public static CanFrame Parse(string line)
    {
        if (line == null)
        {
            return null;
        }

        string[] tokens = line.Trim().Split(' ');
        if (tokens.Length == 0)
        {
            return null;
        }

        uint id;
        try
        {
            id = Convert.ToUInt32(tokens[0], 16);
        }
        catch
        {
            return null;
        }

        if (id > 0x7FF)
        {
            return null;
        }

        byte[] data = new byte[Math.Max(0, tokens.Length - 1)];
        for (int i = 1; i < tokens.Length; i++)
        {
            try
            {
                data[i - 1] = Convert.ToByte(tokens[i], 16);
            }
            catch
            {
                return null;
            }
        }

        return new CanFrame(id, data);
    }
}
