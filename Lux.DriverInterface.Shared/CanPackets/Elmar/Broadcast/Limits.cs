using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct Limits(uint id, float maxOutputVoltage, float maxInputCurrent) : IReadableCanPacket<Limits>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// The maximum output voltage (V)
	/// </summary>
	public float MaxOutputVoltage { get; } = maxOutputVoltage;
	/// <summary>
	/// The maximum input current (A)
	/// </summary>
	public float MaxInputCurrent { get; } = maxInputCurrent;
	
	public static bool IsValidId(uint id, bool isExtended) => !isExtended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.Limits;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Limits packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float maxOutputVoltage = BinaryPrimitives.ReadSingleLittleEndian(a);
		float maxInputCurrent = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(4));

		packet = new Limits(id, maxOutputVoltage, maxInputCurrent);
		return true;
	}
}
