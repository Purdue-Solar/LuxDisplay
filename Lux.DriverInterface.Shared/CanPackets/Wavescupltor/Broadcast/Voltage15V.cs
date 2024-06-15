using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct Voltage15V(float reserved, float voltage15) : IReadableCanPacket<Voltage15V>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.Voltage15V;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Reserved
    /// </summary>
    public float Reserved { get; set; } = reserved;
    /// <summary>
    /// Actual voltage level of the 15V power rail
    /// </summary>
    public float Voltage15 { get; set; } = voltage15;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, [NotNullWhen(true)] out IReadableCanPacket? readableCanPacket)
	{
		if (!TryRead(id, extended, data, out var packet))
		{
			readableCanPacket = null;
			return false;
		}

		readableCanPacket = packet;
		return true;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Voltage15V packet)
    {
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float reserved = BinaryPrimitives.ReadSingleLittleEndian(a);
        float voltage15 = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new Voltage15V(reserved, voltage15);
        return true;
    }
}
