using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
public struct BackEmf(float bemfQ, float bemfD) : IReadableCanPacket<BackEmf>
{
    public static uint CanId => WaveSculptorBase.BroadcastBaseId + (uint)BroadcastId.BackEmf;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

	/// <summary>
	/// The peak of the phase to neutral motor voltage (V)
	/// </summary>
	[FieldLabel("(V)")] 
	public float BemfQ { get; set; } = bemfQ;
	/// <summary>
	/// By definition this value is always 0 (V)
	/// </summary>
	[FieldLabel("(V)")] 
	public float BemfD { get; set; } = bemfD;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out BackEmf packet)
    {
        if (!IsValidId(id, isExtended) || data.Length < Size)
        {
            packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float bq = BinaryPrimitives.ReadSingleLittleEndian(a);
        float bd = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new BackEmf(bq, bd);
        return true;
    }
}
