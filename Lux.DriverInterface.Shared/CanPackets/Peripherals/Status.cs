using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Peripherals;
public readonly struct Status(uint id, Outputs outputs) : IReadableCanPacket<Status>
{
	public PsrCanId CanId { get; } = id;
	public uint Id => CanId.ToInteger();

	public static bool IsExtended => true;
	public static int Size => 2;

	public Outputs Outputs { get; } = outputs;

	public static bool IsValidId(uint id, bool extended)
	{
		if (!extended)
			return false;

		uint mask = (PsrCanId.DeviceTypeMask | PsrCanId.MessageIdMask).ToInteger();
		uint idEq = PsrCanId.ToInteger(0, 0, (byte)MessageId.Status, CanIds.DeviceType.Peripherals, CanIds.MessagePriority.Highest);

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Status packet)
	{

		if (data.Length < Size || !IsValidId(id, isExtended))
		{
			packet = default;
			return false;
		}

		ushort outputs = BinaryPrimitives.ReadUInt16LittleEndian(data);

		packet = new Status(id, new Outputs(outputs));
		return true;
	}
}
