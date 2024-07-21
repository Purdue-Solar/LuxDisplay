using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct OutputMeasurements(uint id, float voltage, float current) : IReadableCanPacket<OutputMeasurements>, IWriteableCanPacket<OutputMeasurements>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// The voltage at the output (V)
	/// </summary>
	[FieldLabel("(V)")] 
	public float OutputVoltage { get; } = voltage;
	/// <summary>
	/// The output current (A)
	/// </summary>
	[FieldLabel("(A)")] 
	public float OutputCurrent { get; } = current;

	public static bool IsValidId(uint id, bool isExtended) => !isExtended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.OutputMeasurements;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out OutputMeasurements packet)
	{
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float voltage = BinaryPrimitives.ReadSingleLittleEndian(a);
		float current = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(4));

		packet = new OutputMeasurements(id, voltage, current);
		return true;
	}

	public bool TryWrite(Span<byte> data, out int written)
	{
		if (data.Length < Size)
		{
			written = 0;
			return false;
		}

		BinaryPrimitives.WriteSingleLittleEndian(data, OutputVoltage);
		BinaryPrimitives.WriteSingleLittleEndian(data.Slice(4), OutputCurrent);

		written = Size;
		return true;
	}
}
