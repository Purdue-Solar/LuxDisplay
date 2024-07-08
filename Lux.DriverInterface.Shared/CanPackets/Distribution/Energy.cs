using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Distribution;

public readonly struct Energy(uint id, uint energy, float scale) : IReadableCanPacket<Energy>
{
	public PsrCanId CanId { get; } = id;

	public uint Id => CanId.ToInteger();

	public uint EnergyValue { get; } = energy;
	public float Scale { get; } = scale;

	public static int Size => 8;
	public static bool IsExtended => true;

	public static bool IsValidId(uint id, bool extended)
	{
		if (!extended)
			return false;

		uint mask = (PsrCanId.DeviceTypeMask | PsrCanId.MessageIdMask).ToInteger();
		uint idEqMain = PsrCanId.ToInteger(0, 0, (byte)MessageId.MainEnergy, CanIds.DeviceType.Distribution, CanIds.MessagePriority.Highest);
		uint idEqAux = PsrCanId.ToInteger(0, 0, (byte)MessageId.AuxEnergy, CanIds.DeviceType.Distribution, CanIds.MessagePriority.Highest);

		uint masked = id & mask;

		return masked == idEqMain || masked == idEqAux;
	}

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Energy packet)
	{
		if (data.Length < Size || !IsValidId(id, isExtended))
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		uint energy = BinaryPrimitives.ReadUInt32LittleEndian(a);
		float scale = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(4));

		packet = new Energy(id, energy, scale);
		return true;
	}
}
