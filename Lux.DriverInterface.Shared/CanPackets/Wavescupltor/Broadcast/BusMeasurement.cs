using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct BusMeasurement(float voltage, float current) : IReadableCanPacket<BusMeasurement>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.BusMeasurement;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;

    public static int Size => 8;

    public float BusVoltage { get; set; } = voltage;
    public float BusCurrent { get; set; } = current;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out BusMeasurement measurement)
    {
        measurement = default;
        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float voltage = BinaryPrimitives.ReadSingleLittleEndian(data);
        float current = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        measurement = new BusMeasurement(voltage, current);
        return true;
    }
}
