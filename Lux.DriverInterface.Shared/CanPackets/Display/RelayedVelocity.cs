using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Display;
public enum MessageId : byte
{
	RelayedVelocity = 0x1
}

public readonly struct RelayedVelocity(uint id, float velocity) : IWriteableCanPacket<RelayedVelocity>
{
	public PsrCanId CanId { get; } = id;
	public uint Id => CanId.ToInteger();

	public static bool IsExtended => true;
	public static int Size => 8;

	public float Velocity { get; } = velocity;
	public float Reserved => 0;

	private static readonly uint IdMask = (PsrCanId.DeviceTypeMask | PsrCanId.MessageIdMask).ToInteger();
	private static readonly uint IdEq = PsrCanId.ToInteger(0, 0, (byte)MessageId.RelayedVelocity, CanIds.DeviceType.Display, 0x00);

	public static PsrCanId DefaultId => new PsrCanId(0xff, CanIds.DisplayBase, (byte)MessageId.RelayedVelocity, CanIds.DeviceType.Display, CanIds.MessagePriority.High);

	public static bool IsValidId(uint id, bool extended) => extended && (id & IdMask) == IdEq;

	public readonly bool TryWrite(Span<byte> buffer, out int written)
	{
		if (buffer.Length < Size)
		{
			written = 0;
			return false;
		}

		Span<byte> b = MemoryMarshal.CreateSpan(ref buffer[0], Size);

		BinaryPrimitives.WriteSingleLittleEndian(b, Velocity);
		BinaryPrimitives.WriteSingleLittleEndian(b.Slice(sizeof(float)), Reserved);

		written = Size;
		return true;
	}
}
