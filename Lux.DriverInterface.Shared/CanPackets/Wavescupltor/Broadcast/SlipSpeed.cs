using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public readonly struct SlipSpeed(float reserved, float slipSpeed) : IReadableCanPacket<SlipSpeed>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.SlipSpeed;
    public uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Reserved (degrees Celcius)
    /// </summary>
    public float Reserved { get; } = reserved;
    /// <summary>
    /// Slip speed when driving an induction motor (Hz)
    /// </summary>
    public float SlipSpeedHz { get; } = slipSpeed;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out SlipSpeed packet)
    {
        packet = default;
        if (!IsValidId(id, isExtended) || data.Length < Size)
            return false;

        float reserved = BinaryPrimitives.ReadSingleLittleEndian(data);
        float slipSpeed = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new SlipSpeed(reserved, slipSpeed);
        return true;
    }
}
