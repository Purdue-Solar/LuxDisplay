using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out VelocityMeasurement packet)
    {
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float motorVelocity = BinaryPrimitives.ReadSingleLittleEndian(a);
        float vehicleVelocity = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new VelocityMeasurement(motorVelocity, vehicleVelocity);
        return true;
    }
}
