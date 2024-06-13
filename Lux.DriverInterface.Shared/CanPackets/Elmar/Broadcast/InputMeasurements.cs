using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct InputMeasurements(uint id, float voltage, float current) : IReadableCanPacket<InputMeasurements>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// The voltage at the input (V)
	/// </summary>
	public float InputVoltage { get; } = voltage;
	/// <summary>
	/// The input current (A)
	/// </summary>
	public float InputCurrent { get; } = current;

	public static bool IsValidId(uint id, bool isExtended) => !isExtended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.InputMeasurements;

	static bool IReadableCanPacket.TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out IReadableCanPacket readableCanPacket)
	{
		bool flag = TryRead(id, isExtended, data, out var packet);
		readableCanPacket = packet;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out InputMeasurements packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		float voltage = BinaryPrimitives.ReadSingleLittleEndian(data);
		float current = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));
		packet = new InputMeasurements(id, voltage, current);
		return true;
	}
}
