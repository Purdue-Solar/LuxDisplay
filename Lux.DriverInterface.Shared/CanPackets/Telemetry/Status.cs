using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Telemetry;
public readonly struct Status(uint id, Status.StatusFlags flags, byte brakePressure1, byte brakePressure2, byte cabinTemp, byte cabinHumidity) : IReadableCanPacket<Status>
{
	public PsrCanId CanId { get; } = id;
	public uint Id => CanId.ToInteger();

	public static bool IsExtended => true;
	public static int Size => 8;

	public StatusFlags Flags { get; } = flags;
	public byte BrakePressure1 { get; } = brakePressure1;
	public byte BrakePressure2 { get; } = brakePressure2;
	public byte CabinTemp { get; } = cabinTemp;
	public byte CabinHumidity { get; } = cabinHumidity;
	public byte Reserved1 { get; } = 0;
	public byte Reserved2 { get; } = 0;


	private static uint IdMask { get; } = (PsrCanId.DeviceTypeMask | PsrCanId.MessageIdMask).ToInteger();
	private static uint IdEq { get; } = PsrCanId.ToInteger(0, 0, (byte)MessageId.Status, CanIds.DeviceType.Telemetry, 0x00);
	public static bool IsValidId(uint id, bool extended) => extended && (id & IdMask) == IdEq;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, [NotNullWhen(true)] out IReadableCanPacket? readableCanPacket)
	{
		if (!TryRead(id, extended, data, out Status packet))
		{
			readableCanPacket = null;
			return false;
		}

		readableCanPacket = packet;
		return true;
	}

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out Status packet)
	{
		if (data.Length < Size || !IsValidId(id, extended))
		{
			packet = default;
			return false;
		}

		// Hack to avoid redundant range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		StatusFlags flags = (StatusFlags)BinaryPrimitives.ReadUInt16LittleEndian(a);
		byte brakePressure1 = a[2];
		byte brakePressure2 = a[3];
		byte cabinTemp = a[4];
		byte cabinHumidity = a[5];

		packet = new Status(id, flags, brakePressure1, brakePressure2, cabinTemp, cabinHumidity);
		return true;
	}

	[Flags]
	public enum StatusFlags : ushort
	{
		BrakesEngaged = 1 << 0,
		TemperatureWarning = 1 << 1,
		TemperatureCritical = 1 << 2
	}
}

public enum MessageId : byte
{
	Status = 0,
	BrakePressures = 1,
	TemperatureHumidity = 2,
	Accelerometer = 3,
	Gyroscope = 4
}
