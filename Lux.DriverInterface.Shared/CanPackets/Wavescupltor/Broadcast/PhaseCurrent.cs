using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct PhaseCurrent(float phaseBCurrent, float phaseCCurent) : IReadableCanPacket<PhaseCurrent>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.PhaseCurrent;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// The current in phase B (A rms)
    /// </summary>
    public float PhaseBCurrent { get; set; } = phaseBCurrent;
    /// <summary>
    /// The current in phase C (A rms)
    /// </summary>
    public float PhaseCCurrent { get; set; } = phaseCCurent;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out PhaseCurrent packet)
    {
        packet = default;

        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float phaseBCurrent = BinaryPrimitives.ReadSingleLittleEndian(data);
        float phaseCCurent = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new PhaseCurrent(phaseBCurrent, phaseCCurent);
        return true;
    }
}
