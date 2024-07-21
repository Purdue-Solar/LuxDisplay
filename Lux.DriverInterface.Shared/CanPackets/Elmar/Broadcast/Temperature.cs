using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
public readonly struct Temperature(uint id, float mosfetTemp, float controllerTemp) : IReadableCanPacket<Temperature>
{
	public uint Id { get; } = id;
	public static bool IsExtended => false;
	public static int Size => 8;

	public byte DeviceId { get; } = (byte)((id & ElmarBase.DeviceIdMask) >> ElmarBase.DeviceIdOffset);
	/// <summary>
	/// Temperature of the MOSFET (deg Celcius)
	/// </summary>
	[FieldLabel("(deg C)")] 
	public float MosfetTemperature { get; } = mosfetTemp;
	/// <summary>
	/// Temperature of the controller (deg Celcius)
	/// </summary>
	[FieldLabel("(deg C)")] 
	public float ControllerTemperature { get; } = controllerTemp;

	public static bool IsValidId(uint id, bool extended) => !extended && (id & ElmarBase.BaseMessageMask) == ElmarBase.BaseId + (uint)BroadcastId.Temperature;

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

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out Temperature packet)
	{
		if (!IsValidId(id, extended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float mosfetTemp = BinaryPrimitives.ReadSingleLittleEndian(a);
		float controllerTemp = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(4));

		packet = new Temperature(id, mosfetTemp, controllerTemp);
		return true;
	}
}
