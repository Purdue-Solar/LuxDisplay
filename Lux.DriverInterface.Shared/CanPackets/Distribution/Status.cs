using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Distribution;

public readonly struct Status(uint id, Status.StatusFlags flags) : IReadableCanPacket<Status>, IWriteableCanPacket<Status>
{
	public PsrCanId CanId { get; } = id;

	public uint Id => CanId.ToInteger();

	public StatusFlags Flags { get; } = flags;
	public ushort Reserved0 { get; } = 0;
	public ushort Reserved1 { get; } = 0;
	public ushort Reserved2 { get; } = 0;

	public static int Size => 8;
	public static bool IsExtended => true;

	public static bool IsValidId(uint id, bool extended)
	{
		if (!extended)
			return false;

		uint mask = (PsrCanId.DeviceTypeMask | PsrCanId.MessageIdMask).ToInteger();
		uint idEq = PsrCanId.ToInteger(0, 0, (byte)MessageId.Status, CanIds.DeviceType.Distribution, CanIds.MessagePriority.Highest);

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

		ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(data);

		packet = new Status(id, (StatusFlags)flags);
		return true;
	}

	public bool TryWrite(Span<byte> buffer, out int bytesWritten)
	{
		if (buffer.Length < Size)
		{
			bytesWritten = 0;
			return false;
		}

		Span<byte> a = MemoryMarshal.CreateSpan(ref buffer[0], Size);

		BinaryPrimitives.WriteUInt16LittleEndian(a, (ushort)Flags);
		BinaryPrimitives.WriteUInt16LittleEndian(a.Slice(2), Reserved0);
		BinaryPrimitives.WriteUInt16LittleEndian(a.Slice(4), Reserved1);
		BinaryPrimitives.WriteUInt16LittleEndian(a.Slice(6), Reserved2);

		bytesWritten = Size;
		return true;
	}

	public enum StatusFlags : ushort
	{
		None = 0,
		DcDcValid = 1 << 0,
		AuxValid = 1 << 1,
		MainCurrentWarning = 1 << 4,
		MainOverCurrent = 1 << 5,
		MainUnderVoltage = 1 << 6,
		MainOverVoltage = 1 << 7,
		AuxCurrentWarning = 1 << 8,
		AuxOverCurrent = 1 << 9,
		AuxUnderVoltage = 1 << 10,
		AuxOverVoltage = 1 << 11,

		Mask = DcDcValid | AuxValid | MainCurrentWarning | MainOverCurrent | MainUnderVoltage | MainOverVoltage | AuxCurrentWarning | AuxOverCurrent | AuxUnderVoltage | AuxOverVoltage
	}
}

public enum MessageId : byte
{
	Status = 0x00,
	BusVoltages = 0x01,
	Currents = 0x02,
	Temperatures = 0x03,
	Powers = 0x04,
	MainEnergy = 0x05,
	AuxEnergy = 0x06
}