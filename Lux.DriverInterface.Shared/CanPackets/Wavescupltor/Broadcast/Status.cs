using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct Status(Status.LimitFlags limits, Status.ErrorFlags errors, ushort activeMotor, byte txErrors, byte rxErrors) : IReadableCanPacket<Status>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.Status;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;

    public static int Size => 8;

    public LimitFlags Limits { get; set; } = limits;
    public ErrorFlags Errors { get; set; } = errors;
    public ushort ActiveMotor { get; set; } = activeMotor;
    public byte TransmitErrorCount { get; set; } = txErrors;
    public byte ReceiveErrorCount { get; set; } = rxErrors;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Status packet)
    {
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		LimitFlags limits = (LimitFlags)BinaryPrimitives.ReadUInt16LittleEndian(a);
        ErrorFlags errors = (ErrorFlags)BinaryPrimitives.ReadUInt16LittleEndian(a.Slice(sizeof(ushort)));
        ushort activeMotor = BinaryPrimitives.ReadUInt16LittleEndian(a.Slice(sizeof(ushort) * 2));
        byte txErrors = a[sizeof(ushort) * 3];
        byte rxErrors = a[sizeof(ushort) * 3 + 1];

        packet = new Status(limits, errors, activeMotor, txErrors, rxErrors);
        return true;
    }

    [Flags]
    public enum ErrorFlags : ushort
    {
        None = 0,
        Reserved0 = 1 << 0,
        SoftwareOverCurrent = 1 << 1,
        DcBusOverVoltage = 1 << 2,
        BadPositionHallSequence = 1 << 3,
        WatchdogReset = 1 << 4,
        ConfigReadError = 1 << 5,
        Uvlo15V = 1 << 6,
        DesaturationFault = 1 << 7,
        MotorOverSpeed = 1 << 8,
    }

    [Flags]
    public enum LimitFlags : ushort
    {
        None = 0,
        OverVoltagePwm = 1 << 0,
        MotorCurrent = 1 << 1,
        Velocity = 1 << 2,
        BusCurrent = 1 << 3,
        BusVoltageUpperLimit = 1 << 4,
        BusVoltageLowerLimit = 1 << 5,
        IpmOrMotorTemperature = 1 << 6,
    }
}
