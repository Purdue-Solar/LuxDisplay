using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavesculptor.Broadcast;
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

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out DspBoardTemp packet)
    {
		if (!IsValidId(id, extended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float dspTemp = BinaryPrimitives.ReadSingleLittleEndian(a);
        float reserved = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new DspBoardTemp(dspTemp, reserved);
        return true;
    }
}
