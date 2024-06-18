using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavesculptor.Command;
public readonly struct Reset : IWriteableCanPacket<Reset>
{
	public static uint CanId => WavesculptorBase.CommandBaseId + (uint)CommandId.Reset;
	public uint Id => CanId;
	public static bool IsExtended => false;
	public static int Size => 8;

	public static bool IsValidId(uint id, bool isExtended) => !isExtended && id == CanId;

	public bool TryWrite(Span<byte> buffer, out int written)
	{
		written = 0;
		if (buffer.Length < Size)
			return false;

		written = Size;
		BinaryPrimitives.WriteUInt64LittleEndian(buffer, 0);

		return true;
	}
}
