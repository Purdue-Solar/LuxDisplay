using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Peripherals;
public readonly struct ChangeOutputs(uint id, Outputs outputs) : IWriteableCanPacket<ChangeOutputs>
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
		uint idEq = PsrCanId.ToInteger(0, 0, (byte)MessageId.ChangeOutputs, CanIds.DeviceType.Peripherals, CanIds.MessagePriority.Highest);

		return (id & mask) == idEq;
	}

	public bool TryWrite(Span<byte> data, out int written)
	{
		if (data.Length < Size)
		{
			written = 0;
			return false;
		}

		written = Size;
		return BinaryPrimitives.TryWriteUInt16LittleEndian(data, Outputs.Value);
	}
}
