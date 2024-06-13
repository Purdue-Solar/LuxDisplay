using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Command;
public readonly struct Mode(uint id, byte mode) : IWriteableCanPacket<Mode>
{
	public uint Id { get; } = id;
	public static bool IsExtended { get; } = false;
	public static int Size { get; } = 1;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// Mode of operation (0 = standby, 1 = active)
	/// </summary>
	public byte ModeType { get; } = mode;

	public static Mode FromDeviceId(byte deviceId, byte mode) => new Mode((ElmarBase.BaseId + (uint)CommandId.Mode) | ((uint)(deviceId & ElmarBase.DeviceIdMaskShifted) << ElmarBase.DeviceIdOffset), mode);

	public static bool IsValidId(uint id, bool extended) => !extended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)CommandId.Mode;

	public bool TryWrite(Span<byte> data, out int written)
	{
		if (data.Length < Size)
		{
			written = 0;
			return false;
		}

		data[0] = ModeType;

		written = Size;
		return true;
	}
}
