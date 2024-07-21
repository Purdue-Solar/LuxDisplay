using Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct AuxiliaryPowerSupply(uint id, float voltage12V, float voltage3V) : IReadableCanPacket<AuxiliaryPowerSupply>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// The voltage of the 12V supply (V)
	/// </summary>
	[FieldLabel("(V)")] 
	public float Voltage12V { get; } = voltage12V;
	/// <summary>
	/// The voltage of the 3V supply (V)
	/// </summary>
	[FieldLabel("(V)")] 
	public float Voltage3V { get; } = voltage3V;

	public static bool IsValidId(uint id, bool isExtended) => !isExtended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.AuxiliaryPowerSupply;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out AuxiliaryPowerSupply packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float voltage12V = BinaryPrimitives.ReadSingleLittleEndian(a);
		float voltage3V = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(4));

		packet = new AuxiliaryPowerSupply(id, voltage12V, voltage3V);
		return true;
	}
}
