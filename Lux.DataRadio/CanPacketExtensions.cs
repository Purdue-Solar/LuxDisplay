using Lux.DriverInterface.Shared;
using SocketCANSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DataRadio;
public static class CanPacketExtensions
{
    public static CanFrame ToCanFrame<T>(this IWriteableCanPacket<T> packet) where T : struct, IWriteableCanPacket<T>
    {
        byte[] buffer = new byte[8];
        packet.TryWrite(buffer, out _);

        uint id = packet.Id;
        if (T.IsExtended)
            id |= 0x80000000;

        CanFrame frame = default;
        frame.Data = buffer;
        frame.CanId = id;
        frame.Length = (byte)T.Size;
        frame.Pad = 0;
        frame.Res0 = 0;
        frame.Len8Dlc = 0;

        return frame;
    }
}
