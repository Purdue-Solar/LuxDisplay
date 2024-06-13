using Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
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
	public float Voltage12V { get; } = voltage12V;
	/// <summary>
	/// The voltage of the 3V supply (V)
	/// </summary>
	public float Voltage3V { get; } = voltage3V;

	public static bool IsValidId(uint id, bool isExtended) => !isExtended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.AuxiliaryPowerSupply;

	static bool IReadableCanPacket.TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out IReadableCanPacket readableCanPacket)
	{
		bool flag = TryRead(id, isExtended, data, out var packet);
		readableCanPacket = packet;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out AuxiliaryPowerSupply packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		float voltage12V = BinaryPrimitives.ReadSingleLittleEndian(data);
		float voltage3V = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));

		packet = new AuxiliaryPowerSupply(id, voltage12V, voltage3V);
		return true;
	}
}
