using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Battery;
public readonly struct CellVoltageAndTemperature(ushort lowCellVoltage, ushort highCellVoltage, byte lowCellId, byte highCellId, byte lowTempId, byte highTempId) : IReadableCanPacket<CellVoltageAndTemperature>
{
	public const uint CanId = 0x203;
	public uint Id => CanId;

	public const float CellVoltageFactor = 0.0001f;

	/// <summary>
	/// Lowest Cell Voltage (0.0001V)
	/// </summary>
	[FieldScale(0.0001)]
	[FieldLabel("(V)")]
	public ushort LowCellVoltage { get; } = lowCellVoltage;
	/// <summary>
	/// Highest Cell Voltage (0.0001V)
	/// </summary>
	[FieldScale(0.0001)]
	[FieldLabel("(V)")]
	public ushort HighCellVoltage { get; } = highCellVoltage;
	public byte LowCellId { get; } = lowCellId;
	public byte HighCellId { get; } = highCellId;
	public byte LowTempId { get; } = lowTempId;
	public byte HighTempId { get; } = highTempId;

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

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out CellVoltageAndTemperature packet)
	{
		if (data.Length < Size || !IsValidId(id, extended))
		{
			packet = default;
			return false;
		}

		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		ushort highCellVoltage = BinaryPrimitives.ReadUInt16LittleEndian(a);
		ushort lowCellVoltage = BinaryPrimitives.ReadUInt16LittleEndian(a.Slice(2));
		byte lowCellId = a[4];
		byte highCellId = a[5];
		byte lowTempId = a[6];
		byte highTempId = a[7];

		packet = new CellVoltageAndTemperature(lowCellVoltage, highCellVoltage, lowCellId, highCellId, lowTempId, highTempId); ;
		return true;
	}

}
