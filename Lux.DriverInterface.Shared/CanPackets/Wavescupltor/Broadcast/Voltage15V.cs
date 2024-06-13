using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct Voltage15V(float reserved, float voltage15) : IReadableCanPacket<Voltage15V>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.Voltage15V;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 4;

    /// <summary>
    /// Reserved
    /// </summary>
    public float Reserved { get; set; } = reserved;
    /// <summary>
    /// Actual voltage level of the 15V power rail
    /// </summary>
    public float Voltage15 { get; set; } = voltage15;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Voltage15V packet)
    {
        packet = default;

        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float reserved = BinaryPrimitives.ReadSingleLittleEndian(data);
        float voltage15 = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new Voltage15V(reserved, voltage15);
        return true;
    }
}
