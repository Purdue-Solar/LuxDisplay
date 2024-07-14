using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Command;
public readonly struct Power(float reserved, float busCurrent) : IWriteableCanPacket<Power>
{
	public static uint CanId => WaveSculptorBase.CommandBaseId + (uint)CommandId.Power;
	public uint Id => CanId;
	public static bool IsExtended => false;
	public static int Size => 8;

	/// <summary>
	/// Reserved
	/// </summary>
	public float Reserved { get; } = reserved;
	/// <summary>
	///	Desired set point of current drawn from the bus by the controller as a percentage of absoluate bus current limit (0-100%)
	///	</summary>
	public float BusCurrent { get; } = busCurrent;

	public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	public bool TryWrite(Span<byte> buffer, out int written)
	{
		written = 0;
		if (buffer.Length < Size)
			return false;

		written = Size;
		BinaryPrimitives.WriteSingleLittleEndian(buffer, Reserved);
		BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(sizeof(float)), BusCurrent);

		return true;
	}
}
