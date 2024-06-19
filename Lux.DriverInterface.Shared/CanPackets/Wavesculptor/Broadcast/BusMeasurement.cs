using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
public struct BusMeasurement(float voltage, float current) : IReadableCanPacket<BusMeasurement>, IWriteableCanPacket<BusMeasurement>
{
    public static uint CanId => WaveSculptorBase.BroadcastBaseId + (uint)BroadcastId.BusMeasurement;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;

    public static int Size => 8;

    public float BusVoltage { get; set; } = voltage;
    public float BusCurrent { get; set; } = current;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out BusMeasurement packet)
    {
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float voltage = BinaryPrimitives.ReadSingleLittleEndian(a);
        float current = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new BusMeasurement(voltage, current);
        return true;
    }

	public readonly bool TryWrite(Span<byte> buffer, out int written)
	{
		if (buffer.Length < Size)
		{
			written = 0;
			return false;
		}

        Span<byte> a = MemoryMarshal.CreateSpan(ref buffer[0], Size);

        BinaryPrimitives.WriteSingleLittleEndian(a, BusVoltage);
        BinaryPrimitives.WriteSingleLittleEndian(a.Slice(sizeof(float)), BusCurrent);

        written = Size;
		return true;
    }
}
