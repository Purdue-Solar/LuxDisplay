using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct Temperature(uint id, float mosfetTemp, float controllerTemp) : IReadableCanPacket<Temperature>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// Temperature of the MOSFET (deg Celcius)
	/// </summary>
	public float MosfetTemperature { get; } = mosfetTemp;
	/// <summary>
	/// Temperature of the controller (deg Celcius)
	/// </summary>
	public float ControllerTemperature { get; } = controllerTemp;

	public static bool IsValidId(uint id, bool extended) => !extended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.Temperature;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket readablePacket)
	{
		bool flag = TryRead(id, extended, data, out var packet);
		readablePacket = packet;
		return flag;
	}

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out Temperature packet)
	{
		if (!IsValidId(id, extended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		float mosfetTemp = BinaryPrimitives.ReadSingleLittleEndian(data);
		float controllerTemp = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));

		packet = new Temperature(id, mosfetTemp, controllerTemp);
		return true;
	}
}
