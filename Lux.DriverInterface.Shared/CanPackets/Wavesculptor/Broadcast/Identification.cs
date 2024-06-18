using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavesculptor.Broadcast;
public struct Identification(uint tritiumId, uint serialNumber) : IReadableCanPacket<Identification>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.Identification;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    public uint TritiumId { get; set; } = tritiumId;
    public uint SerialNumber { get; set; } = serialNumber;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Identification packet)
    {
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		uint tritiumId = BinaryPrimitives.ReadUInt32LittleEndian(a);
        uint serialNumber = BinaryPrimitives.ReadUInt32LittleEndian(a.Slice(sizeof(uint)));

        packet = new Identification(serialNumber, tritiumId);
        return true;
    }
}
