using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Battery;
public readonly struct PackAmpHours(ushort packAmpHours, ushort adaptiveAmpHours) : IReadableCanPacket<PackAmpHours>
{
	public const uint CanId = 0x202;
	public uint Id => CanId;

	public const float AmpHoursFactor = 0.1f;

	/// <summary>
	/// Pack Amp Hours (0.1Ah)
	/// </summary>
	[FieldScale(0.1)]
	[FieldLabel("(Ah)")]
	public ushort AmpHours { get; } = packAmpHours;
	/// <summary>
	/// Adaptive Pack Amp Hours (0.1Ah)
	/// </summary>
	[FieldScale(0.1)]
	[FieldLabel("(Ah)")]
	public ushort AdaptiveAmpHours { get; } = adaptiveAmpHours;

	public static bool IsExtended => false;
	public static int Size => 8;

	public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, [NotNullWhen(true)] out IReadableCanPacket? readableCanPacket)
	{
		if (!TryRead(id, extended, data, out var packet))
		{
			readableCanPacket = null;
			return false;
		}

		readableCanPacket = packet;
		return true;
	}

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out PackAmpHours packet)
	{
		if (data.Length < Size || !IsValidId(id, extended))
		{
			packet = default;
			return false;
		}

		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		ushort ampHours = BinaryPrimitives.ReadUInt16LittleEndian(a);
		ushort adaptiveAmpHours = BinaryPrimitives.ReadUInt16LittleEndian(a.Slice(2));

		packet = new PackAmpHours(ampHours, adaptiveAmpHours);
		return true;
	}

}
