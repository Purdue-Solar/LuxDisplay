using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct HeatSinkMotorTemp(float motorTemp, float heatSinkTemp) : IReadableCanPacket<HeatSinkMotorTemp>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.HeatSinkMotorTemp;
    public readonly uint Id => CanId;
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

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out HeatSinkMotorTemp packet)
    {
		if (!IsValidId(id, extended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float motorTemp = BinaryPrimitives.ReadSingleLittleEndian(a);
        float heatSinkTemp = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new HeatSinkMotorTemp(motorTemp, heatSinkTemp);
        return true;
    }
}
