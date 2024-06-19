using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
public struct PhaseCurrent(float phaseBCurrent, float phaseCCurent) : IReadableCanPacket<PhaseCurrent>
{
    public static uint CanId => WaveSculptorBase.BroadcastBaseId + (uint)BroadcastId.PhaseCurrent;
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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out PhaseCurrent packet)
    {
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float phaseBCurrent = BinaryPrimitives.ReadSingleLittleEndian(a);
        float phaseCCurent = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new PhaseCurrent(phaseBCurrent, phaseCCurent);
        return true;
    }
}
