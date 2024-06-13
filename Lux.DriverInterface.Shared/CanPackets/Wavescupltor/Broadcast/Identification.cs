using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct Identification(uint tritiumId, uint serialNumber) : IReadableCanPacket<Identification>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.Identification;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    public uint TritiumId { get; set; } = tritiumId;
    public uint SerialNumber { get; set; } = serialNumber;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Identification packet)
    {
        packet = default;

        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        uint tritiumId = BinaryPrimitives.ReadUInt32LittleEndian(data);
        uint serialNumber = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(sizeof(uint)));

        packet = new Identification(serialNumber, tritiumId);
        return true;
    }
}
