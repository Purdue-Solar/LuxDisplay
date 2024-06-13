using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public readonly struct DspBoardTemp(float dspBoardTemp, float reserved) : IReadableCanPacket<DspBoardTemp>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.DspBoardTemp;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Temperature of the DSP board (degrees Celsius)
    /// </summary>
    public float DspTemp { get; } = reserved;
    /// <summary>
    /// Reserved
    /// </summary>
    public float Reserved { get; } = dspBoardTemp;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out DspBoardTemp packet)
    {
        packet = default;

        if (extended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float dspTemp = BinaryPrimitives.ReadSingleLittleEndian(data);
        float reserved = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new DspBoardTemp(dspTemp, reserved);
        return true;
    }
}
