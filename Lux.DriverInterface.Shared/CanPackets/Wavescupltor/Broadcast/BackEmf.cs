using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct BackEmf(float bemfQ, float bemfD) : IReadableCanPacket<BackEmf>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.BackEmf;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// The peak of the phase to neutral motor voltage (V)
    /// </summary>
    public float BemfQ { get; set; } = bemfQ;
    /// <summary>
    /// By definition this value is always 0 (V)
    /// </summary>
    public float BemfD { get; set; } = bemfD;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

    static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
    {
        bool flag = TryRead(id, extended, data, out var genericPacket);
        packet = genericPacket;
        return flag;
    }

    public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out BackEmf packet)
    {
        packet = default;

        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float bq = BinaryPrimitives.ReadSingleLittleEndian(data);
        float bd = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new BackEmf(bq, bd);
        return true;
    }
}
