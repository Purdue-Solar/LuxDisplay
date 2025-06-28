using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Battery;
public readonly struct PackAmpHoursFaults(ushort packAmpHours, ushort adaptiveAmpHours, PackAmpHoursFaults.DtcCodes dtcCodes) : IReadableCanPacket<PackAmpHoursFaults>
{
	public const uint CanId = 0x202;
	public uint Id => CanId;

	public const float AmpHoursFactor = 0.1f;

	/// <summary>
	/// Pack Amp Hours (0.1Ah)
	/// </summary>
	[FieldScale(0.1)]
	[FieldLabel("(Ah)")]
	public ushort AmpHours { get; } = packAmpHours;
	/// <summary>
	/// Adaptive Pack Amp Hours (0.1Ah)
	/// </summary>
	[FieldScale(0.1)]
	[FieldLabel("(Ah)")]
	public ushort AdaptiveAmpHours { get; } = adaptiveAmpHours;

	public DtcCodes FaultCodes { get; } = dtcCodes;

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

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out PackAmpHoursFaults packet)
	{
		if (data.Length < Size || !IsValidId(id, extended))
		{
			packet = default;
			return false;
		}

		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		ushort ampHours = BinaryPrimitives.ReadUInt16LittleEndian(a);
		ushort adaptiveAmpHours = BinaryPrimitives.ReadUInt16LittleEndian(a.Slice(2));
		DtcCodes dtcCodes = (DtcCodes)BinaryPrimitives.ReadUInt32LittleEndian(a.Slice(4));

		packet = new PackAmpHoursFaults(ampHours, adaptiveAmpHours, dtcCodes);
		return true;
	}

	[Flags]
	public enum DtcCodes : uint
	{
		DischargeLimitEnforcementFault = 1 << 0,
		ChargerSafetyRelayFault = 1 << 1,
		InternalHardwareFault = 1 << 2,
		InternalHeatsinkThermistor = 1 << 3,
		InternalSoftwareFault = 1 << 4,
		HighestCellVoltageTooHighFault = 1 << 5,
		LowestCellVoltageTooLowFault = 1 << 6,
		PackTooHotFault = 1 << 7,
		// 8-15 reserved
		InternalCommunicationFault = 1 << 16,
		CellBalancingStuckOffFault = 1 << 17,
		WeakCellFault = 1 << 18,
		LowCellVoltageFault = 1 << 19,
		OpenWiringFault = 1 << 20,
		CurrentSensor = 1 << 21,
		HighestCellVoltageOver5VFault = 1 << 22,
		CellAsicFault = 1 << 23,
		WeakPackFault = 1 << 24,
		FanMonitorFault = 1	<< 25,
		ThermistorFault = 1 << 26,
		ExternalCommunicationFault = 1 << 27,
		RedundantPowerSupplyFault = 1 << 28,
		HighVoltageIsolationFault = 1 << 29,
		InputPowerSupplyFault = 1 << 30,
		ChargeLimitEnforcementFault = 1U << 31
	}
}
