using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct HeatSinkMotorTemp(float motorTemp, float heatSinkTemp) : IReadableCanPacket<HeatSinkMotorTemp>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.HeatSinkMotorTemp;
    public uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Internal temperature of the motor
    /// </summary>
    public float MotorTemp { get; set; } = motorTemp;
    /// <summary>
    /// Internal temperature of the Heat-sink (case)
    /// </summary>
    public float HeatSinkTemp { get; set; } = heatSinkTemp;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out HeatSinkMotorTemp packet)
    {
        packet = default;
        if (!IsValidId(id, extended) || data.Length < Size)
            return false;

        float motorTemp = BinaryPrimitives.ReadSingleLittleEndian(data);
        float heatSinkTemp = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new HeatSinkMotorTemp(motorTemp, heatSinkTemp);
        return true;
    }

}
