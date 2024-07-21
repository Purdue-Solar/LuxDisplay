using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Battery;
public readonly struct Status2(byte packDcl, byte packCcl, byte lowTemperature, byte highTemperature, Status2.CurrentLimitStatus currentLimits, short packKwPower) : IReadableCanPacket<Status2>
{
	public const uint CanId = 0x201;
	public uint Id => CanId;

	public const float PackKwPowerFactor = 1f;

	/// <summary>
	/// Pack discharge current limit (1A)
	/// </summary>
	[FieldLabel("(A)")] 
	public byte PackDcl { get; } = packDcl;
	/// <summary>
	/// Pack charge current limit (1A)
	/// </summary>
	[FieldLabel("(A)")] 
	public byte PackCcl { get; } = packCcl;
	/// <summary>
	/// Lowest cell temperature (1 deg C)
	/// </summary>

	[FieldLabel("(deg C)")]
	public byte LowTemperature { get; } = lowTemperature;
	/// <summary>
	/// Highest cell temperature (1 deg C)
	/// </summary>
	[FieldLabel("(deg C)")] 
	public byte HighTemperature { get; } = highTemperature;
	public CurrentLimitStatus CurrentLimits { get; } = currentLimits;
	/// <summary>
	/// Pack power (1W)
	/// </summary>
	public short PackKwPower { get; } = packKwPower;

	public static bool IsExtended => false;
	public static int Size => 8;

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

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out Status2 packet)
	{
		if (data.Length < Size || !IsValidId(id, extended))
		{
			packet = default;
			return false;
		}

		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		byte packDcl = a[0];
		byte packCcl = a[1];
		byte lowTemp = a[2];
		byte highTemp = a[3];
		CurrentLimitStatus currentLimits = (CurrentLimitStatus)BinaryPrimitives.ReadUInt16LittleEndian(a.Slice(4));
		short packKwPower = BinaryPrimitives.ReadInt16LittleEndian(a.Slice(6));

		packet = new Status2(packDcl, packCcl, lowTemp, highTemp, currentLimits, packKwPower);
		return true;
	}

	[Flags]
	public enum CurrentLimitStatus : ushort
	{
		None = 0,
		DclReducedLowSoc = 1 << 0,
		DclReducedHighResistance = 1 << 1,
		DclReducedTemperature = 1 << 2,
		DclReducedLowCellVoltage = 1 << 3,
		DclReducedLowPackVoltage = 1 << 4,
		DclCclReducedVoltageFailsafe = 1 << 6,
		DclCclReducedCommunicationFailsafe = 1 << 7,
		CclReducedHighSoc = 1 << 9,
		CclReducedHighResistance = 1 << 10,
		CclReducedTemperature = 1 << 11,
		CclReducedHighCellVoltage = 1 << 12,
		CclReducedHighPackVoltage = 1 << 13,
		CclReducedChargerLatch = 1 << 14,
		CclReducedAlternateCurrentLimit = 1 << 15
	}
}
