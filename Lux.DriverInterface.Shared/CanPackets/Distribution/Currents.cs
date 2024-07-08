using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Distribution;

public readonly struct Currents(uint id, short mainCurrent, short auxCurrent, float scale) : IReadableCanPacket<Currents>, IWriteableCanPacket<Currents>
{
	public PsrCanId CanId { get; } = id;

	public uint Id => CanId.ToInteger();

	public short MainCurrent { get; } = mainCurrent;
	public short AuxCurrent { get; } = auxCurrent;
	public float Scale { get; } = scale;

	public static int Size => 8;
	public static bool IsExtended => true;

	public static bool IsValidId(uint id, bool extended)
	{
		if (!extended)
			return false;

		uint mask = (PsrCanId.DeviceTypeMask | PsrCanId.MessageIdMask).ToInteger();
		uint idEq = PsrCanId.ToInteger(0, 0, (byte)MessageId.Currents, CanIds.DeviceType.Distribution, CanIds.MessagePriority.Highest);

		return (id & mask) == idEq;
	}

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Currents packet)
	{
		if (data.Length < Size || !IsValidId(id, isExtended))
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		short mainCurrent = BinaryPrimitives.ReadInt16LittleEndian(a);
		short auxCurrent = BinaryPrimitives.ReadInt16LittleEndian(a.Slice(2));
		float scale = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(4));

		packet = new Currents(id, mainCurrent, auxCurrent, scale);
		return true;
	}

	public bool TryWrite(Span<byte> buffer, out int written)
	{
		if (buffer.Length < Size)
		{
			written = 0;
			return false;
		}

		Span<byte> a = MemoryMarshal.CreateSpan(ref buffer[0], Size);

		BinaryPrimitives.WriteInt16LittleEndian(a, MainCurrent);
		BinaryPrimitives.WriteInt16LittleEndian(a.Slice(2), AuxCurrent);
		BinaryPrimitives.WriteSingleLittleEndian(a.Slice(4), Scale);

		written = Size;
		return true;
	}
}
