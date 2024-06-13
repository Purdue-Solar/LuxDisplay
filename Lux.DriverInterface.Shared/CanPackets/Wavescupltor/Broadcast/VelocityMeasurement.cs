using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct VelocityMeasurement(float motorVelocity, float vehicleVelocity) : IReadableCanPacket<VelocityMeasurement>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.VelocityMeasurement;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Motor velocity in RPM
    /// </summary>
    public float MotorVelocity { get; set; } = motorVelocity;
    /// <summary>
    /// Vehicle velocity in m/s
    /// </summary>
    public float VehicleVelocity { get; set; } = vehicleVelocity;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out VelocityMeasurement packet)
    {
        packet = default;

        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float motorVelocity = BinaryPrimitives.ReadSingleLittleEndian(data);
        float vehicleVelocity = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new VelocityMeasurement(motorVelocity, vehicleVelocity);
        return true;
    }
}
