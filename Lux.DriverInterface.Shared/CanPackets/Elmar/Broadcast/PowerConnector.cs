using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct PowerConnector(uint id, float outputVoltage, float connectorTemp) : IReadableCanPacket<PowerConnector>
{
	public uint Id { get; } = id;
	public static bool IsExtended { get; } = false;
	public static int Size { get; } = 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// Output Voltage (Battery side of fuse) (V)
	/// </summary>
	public float OutputVoltage { get; } = outputVoltage;
	/// <summary>
	/// Power connector temperature (deg Celcius)
	/// </summary>
	public float ConnectorTemperature { get; } = connectorTemp;

	public static bool IsValidId(uint id, bool extended) => !extended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.PowerConnector;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket readableCanPacket)
	{
		bool flag = TryRead(id, extended, data, out PowerConnector packet);
		readableCanPacket = packet;
		return flag;
	}

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out PowerConnector packet)
	{
		if (!IsValidId(id, extended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		float outputVoltage = BinaryPrimitives.ReadSingleLittleEndian(data);
		float connectorTemp = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(4));

		packet = new PowerConnector(id, outputVoltage, connectorTemp);
		return true;
	}
}
