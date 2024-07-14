using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Steering;
public readonly struct Status(uint id, Status.ButtonFlags buttons, byte page, byte reserved, float targetSpeed) : IReadableCanPacket<Status>, IWriteableCanPacket<Status>
{
	public PsrCanId CanId { get; } = id;
	public uint Id => CanId.ToInteger();

	public static bool IsExtended => true;
	public static int Size => 8;

	public ButtonFlags Buttons { get; } = buttons;
	public byte Page { get; } = page;
	public byte Reserved { get; } = reserved;
	public float TargetSpeed { get; } = targetSpeed;

	private static uint IdMask { get; } = (PsrCanId.DeviceTypeMask | PsrCanId.MessageIdMask).ToInteger();
	private static uint IdEq { get; } = PsrCanId.ToInteger(0, 0, (byte)MessageId.Status, CanIds.DeviceType.Steering, 0x00);

	public static bool IsValidId(uint id, bool extended) => extended && (id & IdMask) == IdEq;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, [NotNullWhen(true)] out IReadableCanPacket? readableCanPacket)
	{
		if (!TryRead(id, extended, data, out Status packet))
		{
			readableCanPacket = null;
			return false;
		}

		readableCanPacket = packet;
		return true;
	}

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out Status packet)
	{
		if (data.Length < Size || !IsValidId(id, extended))
		{
			packet = default;
			return false;
		}

		// Hack to avoid redundant range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		ButtonFlags buttons = (ButtonFlags)BinaryPrimitives.ReadUInt16LittleEndian(a);
		byte page = a[2];
		byte reserved = a[3];
		float targetSpeed = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(4));

		packet = new Status(id, buttons, page, reserved, targetSpeed);
		return true;
	}

	public readonly bool TryWrite(Span<byte> buffer, out int written)
	{
		if (buffer.Length < Size)
		{
			written = 0;
			return false;
		}

		BinaryPrimitives.WriteUInt16LittleEndian(buffer, (ushort)Buttons);
		buffer[sizeof(ushort)] = Page;
		buffer[sizeof(ushort) + 1] = Reserved;
		BinaryPrimitives.WriteSingleLittleEndian(buffer.Slice(4), TargetSpeed);

		written = Size;
		return true;
	}

	[Flags]
	public enum ButtonFlags : ushort
	{
		None = 0,
		PushToTalk = 1 << 0,
		Headlight = 1 << 1,
		RightTurn = 1 << 2,
		Hazards = 1 << 3,
		LeftTurn = 1 << 4,
		Cruise = 1 << 5,
		CruiseUp = 1 << 6,
		CruiseDown = 1 << 7,
		Horn = 1 << 8
	}
}
