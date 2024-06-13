using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
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

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Status status)
    {
        status = default;
        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        LimitFlags limits = (LimitFlags)BinaryPrimitives.ReadUInt16LittleEndian(data);
        ErrorFlags errors = (ErrorFlags)BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(sizeof(ushort)));
        ushort activeMotor = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(sizeof(ushort) * 2));
        byte txErrors = data[sizeof(ushort) * 3];
        byte rxErrors = data[sizeof(ushort) * 3 + 1];

        status = new Status(limits, errors, activeMotor, txErrors, rxErrors);
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
