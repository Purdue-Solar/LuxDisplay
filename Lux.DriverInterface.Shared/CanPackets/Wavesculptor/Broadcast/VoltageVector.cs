using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
public struct VoltageVector(float vq, float vd) : IReadableCanPacket<VoltageVector>
{
	public static uint CanId => WaveSculptorBase.BroadcastBaseId + (uint)BroadcastId.VoltageVector;
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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out VoltageVector packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float vq = BinaryPrimitives.ReadSingleLittleEndian(a);
		float vd = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

		packet = new VoltageVector(vq, vd);
		return true;
	}
}
