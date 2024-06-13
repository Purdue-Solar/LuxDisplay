using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct OutputMeasurements(uint id, float voltage, float current) : IReadableCanPacket<OutputMeasurements>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// The voltage at the output (V)
	/// </summary>
	public float OutputVoltage { get; } = voltage;
	/// <summary>
	/// The output current (A)
	/// </summary>
	public float OutputCurrent { get; } = current;

	public static bool IsValidId(uint id, bool isExtended) => !isExtended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.OutputMeasurements;

	static bool IReadableCanPacket.TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out IReadableCanPacket readableCanPacket)
	{
		bool flag = TryRead(id, isExtended, data, out var packet);
		readableCanPacket = packet;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out OutputMeasurements packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		float voltage = BinaryPrimitives.ReadSingleLittleEndian(data);
		float current = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));

		packet = new OutputMeasurements(id, voltage, current);
		return true;
	}
}
