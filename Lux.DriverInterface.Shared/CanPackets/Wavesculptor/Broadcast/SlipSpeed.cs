using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
public readonly struct SlipSpeed(float reserved, float slipSpeed) : IReadableCanPacket<SlipSpeed>
{
	public static uint CanId => WaveSculptorBase.BroadcastBaseId + (uint)BroadcastId.SlipSpeed;
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
	[FieldLabel("(Hz)")] 
	public float SlipSpeedHz { get; } = slipSpeed;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out SlipSpeed packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float reserved = BinaryPrimitives.ReadSingleLittleEndian(a);
		float slipSpeed = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

		packet = new SlipSpeed(reserved, slipSpeed);
		return true;
	}
}
