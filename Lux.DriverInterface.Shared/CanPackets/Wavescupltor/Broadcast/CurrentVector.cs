using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct CurrentVector(float iq, float id) : IReadableCanPacket<CurrentVector>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.CurrentVector;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Imaginary component of the applied non-rotating current vector to the motor (A)
    /// </summary>
    public float CurrentQ { get; set; } = iq;
    /// <summary>
    /// Real component of the applied non-rotating voltage current vector to the motor (A)
    /// </summary>
    public float CurrentD { get; set; } = id;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out CurrentVector packet)
    {
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float iQ = BinaryPrimitives.ReadSingleLittleEndian(a);
        float iD = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new CurrentVector(iQ, iD);
        return true;
    }
}
