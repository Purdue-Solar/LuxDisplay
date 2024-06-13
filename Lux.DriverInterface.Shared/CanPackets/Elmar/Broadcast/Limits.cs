using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
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

	static bool IReadableCanPacket.TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out IReadableCanPacket readableCanPacket)
	{
		bool flag = TryRead(id, isExtended, data, out var packet);
		readableCanPacket = packet;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Limits packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		float maxOutputVoltage = BinaryPrimitives.ReadSingleLittleEndian(data);
		float maxInputCurrent = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));

		packet = new Limits(id, maxOutputVoltage, maxInputCurrent);
		return true;
	}
}
