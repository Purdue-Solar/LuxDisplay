using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
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

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out CurrentVector packet)
    {
        packet = default;

        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float iQ = BinaryPrimitives.ReadSingleLittleEndian(data);
        float iD = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new CurrentVector(iQ, iD);
        return true;
    }
}
