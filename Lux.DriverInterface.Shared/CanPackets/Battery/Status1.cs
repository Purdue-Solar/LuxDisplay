using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Battery;
public readonly struct Status1(short current, ushort voltage, byte soc, Status1.RelayState relayState, Status1.FailsafeStatus failsafeStatus, byte averageTemperature) : IReadableCanPacket<Status1>
{
	public const uint CanId = 0x200;
	public uint Id => CanId;

	public const float CurrentFactor = 0.1f;
	public const float VoltageFactor = 0.1f;
	public const float StateOfChargeFactor = 0.5f;

	/// <summary>
	/// Pack instantaneous current (0.1A)
	/// </summary>
	[FieldScale(0.1)]
	[FieldLabel("(A)")] 
	public short Current { get; } = current;
	/// <summary>
	/// Pack instantaneous voltage (0.1V)
	/// </summary>
	[FieldScale(0.1)]
	[FieldLabel("(A)")] 
	public ushort Voltage { get; } = voltage;
	/// <summary>
	/// Pack state of charge (0.5%)
	/// </summary>
	[FieldScale(0.5)]
	[FieldLabel("(%)")] 
	public byte StateOfCharge { get; } = soc;
	public RelayState Relays { get; } = relayState;
	public FailsafeStatus Failsafe { get; } = failsafeStatus;
	/// <summary>
	/// Average temperature of cells (1 deg C)
	/// </summary>
	[FieldLabel("(deg C)")]
	public byte AverageTemperature { get; } = averageTemperature;

	public static bool IsExtended => false;
	public static int Size => 8;

	public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, [NotNullWhen(true)] out IReadableCanPacket? readableCanPacket)
	{
		if (!TryRead(id, isExtended, data, out var packet))
		{
			readableCanPacket = null;
			return false;
		}

		readableCanPacket = packet;
		return true;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Status1 packet)
	{
		if (data.Length < Size || !IsValidId(id, isExtended))
		{
			packet = default;
			return false;
		}

		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(data[0], Size);

		short current = BinaryPrimitives.ReadInt16LittleEndian(a);
		ushort voltage = BinaryPrimitives.ReadUInt16LittleEndian(a.Slice(2));
		byte soc = a[4];
		RelayState relayState = (RelayState)a[5];
		FailsafeStatus failsafe = (FailsafeStatus)a[6];
		byte averageTemperature = a[7];

		packet = new Status1(current, voltage, soc, relayState, failsafe, averageTemperature);
		return true;
	}

	[Flags]
	public enum RelayState : byte
	{
		DischargeEnable = 1 << 0,
		ChargeEnable = 1 << 1,
		ChargerSafety = 1 << 2,
		MalfunctionIndicatorActive = 1 << 3,
		MultipurposeInputSignalStatus = 1 << 4,
		AlwaysOnSignalStatus = 1 << 5,
		IsReadySignalStatus = 1 << 6,
		IsChargingSignalStatus = 1 << 7,
	}

	[Flags]
	public enum FailsafeStatus : byte
	{
		VoltageFailsafeActive = 1 << 0,
		CurrentFailsafeActive = 1 << 1,
		RelayFailsafeActive = 1 << 2,
		CellBalancingActive = 1 << 3,
		ChargeInterlockFailsafeActive = 1 << 4,
		ThermistorBValueTableInvalid = 1 << 5,
		InputPowerSupplyFailsafeActive = 1 << 6
	}
}
