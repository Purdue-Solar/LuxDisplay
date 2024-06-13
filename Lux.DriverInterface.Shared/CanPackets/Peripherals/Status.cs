using System;
using System.Collections.Generic;
using System.Linq;
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

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Status packet)
	{
		packet = default;

		if (!isExtended)
			return false;

		if (!IsValidId(id, isExtended))
			return false;

		if (data.Length < Size)
			return false;

		ushort outputs = BitConverter.ToUInt16(data);
		packet = new Status(id, new Outputs(outputs));
		return true;
	}
}
