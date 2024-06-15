using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct Status(uint id, byte rxErrorCount, byte txErrorCount, byte txOverflowCount, Status.ErrorFlags errorFlags, Status.LimitFlags limitFlags, byte mode, byte reserved, byte testCounter) : IReadableCanPacket<Status>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// CAN RX error count
	/// </summary>
	public byte RxErrorCount { get; } = rxErrorCount;
	/// <summary>
	/// CAN TX error count
	/// </summary>
	public byte TxErrorCount { get; } = txErrorCount;
	/// <summary>
	/// CAN TX overflow count
	/// </summary>
	public byte TxOverflowCount { get; } = txOverflowCount;
	/// <summary>
	/// Error flags
	/// </summary>
	public ErrorFlags Errors { get; } = errorFlags;
	/// <summary>
	/// Limit flags
	/// </summary>
	public LimitFlags Limits { get; } = limitFlags;
	/// <summary>
	/// Mode (0 = standby, 1 = active)
	/// </summary>
	public byte Mode { get; } = mode;
	/// <summary>
	/// Reserved
	/// </summary>
	public byte Reserved { get; } = reserved;
	/// <summary>
	/// Test counter (incremented every second)
	/// </summary>
	public byte TestCounter { get; } = testCounter;

	public static bool IsValidId(uint id, bool extended) => !extended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.Status;
	
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

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out Status packet)
	{
		if (!IsValidId(id, extended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		byte rxErrorCount = data[0];
		byte txErrorCount = data[1];
		byte txOverflowCount = data[2];
		ErrorFlags errorFlags = (ErrorFlags)data[3];
		LimitFlags limitFlags = (LimitFlags)data[4];
		byte mode = data[5];
		byte reserved = data[6];
		byte testCounter = data[7];

		packet = new Status(id, rxErrorCount, txErrorCount, txOverflowCount, errorFlags, limitFlags, mode, reserved, testCounter);
		return true;
	}

	[Flags]
	public enum ErrorFlags : byte
	{
		None = 0,
		LowArrayPower = 1 << 0,
		MosfetOverheat = 1 << 1,
		BatteryLow = 1 << 2,
		BatteryFull = 1 << 3,
		Undervoltage12V = 1 << 4,
		Reserved = 1 << 5,
		HwOvercurrent = 1 << 6,
		HwOvervoltage = 1 << 7
	}

	[Flags]
	public enum LimitFlags : byte
	{
		None = 0,
		InputCurrentMin = 1 << 0,
		InputCurrentMax = 1 << 1,
		OutputVoltageMax = 1 << 2,
		MosfetTemperature = 1 << 3,
		DutyCycleMin = 1 << 4,
		DutyCycleMax = 1 << 5,
		LocalMppt = 1 << 6,
		GlobalMppt = 1 << 7
	}
}
