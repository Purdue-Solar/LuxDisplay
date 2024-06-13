using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Command;
public readonly struct MaxInputCurrent(uint id, float maxCurrent) : IWriteableCanPacket<MaxInputCurrent>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 4;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// Maximum input current (A)
	/// </summary>
	public float MaxCurrent { get; } = maxCurrent;

	public static MaxOutputVoltage FromDeviceId(byte deviceId, float maxVoltage) => new MaxOutputVoltage((ElmarBase.BaseId + (uint)CommandId.MaxInputCurrent) | ((uint)(deviceId & ElmarBase.DeviceIdMaskShifted) << ElmarBase.DeviceIdOffset), maxVoltage);

	public static bool IsValidId(uint id, bool extended) => !extended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)CommandId.MaxInputCurrent;

	public bool TryWrite(Span<byte> data, out int written)
	{
		if (data.Length < Size)
		{
			written = 0;
			return false;
		}

		BinaryPrimitives.WriteSingleLittleEndian(data, MaxCurrent);

		written = Size;
		return true;
	}
}