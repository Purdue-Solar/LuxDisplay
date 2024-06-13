using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct VoltageVector(float vq, float vd) : IReadableCanPacket<VoltageVector>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.VoltageVector;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Imaginary component of the applied non-rotating voltage vector to the motor (V)
    /// </summary>
    public float Vq { get; set; } = vq;
    /// <summary>
    /// Real component of the applied non-rotating voltage vector to the motor (V)
    /// </summary>
    public float Vd { get; set; } = vd;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out VoltageVector packet)
    {
        packet = default;

        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float vq = BinaryPrimitives.ReadSingleLittleEndian(data);
        float vd = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new VoltageVector(vq, vd);
        return true;
    }
}
