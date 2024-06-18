using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavesculptor.Broadcast;
public readonly struct OdometerBusAmpHrs(float odometer, float dcBusAmpHrs) : IReadableCanPacket<OdometerBusAmpHrs>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.OdometerBusAmpHrs;
    public uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Distance the vehicle has travelled since reset (m)
    /// </summary>
    public float Odometer { get; } = odometer;
    /// <summary>
    /// Charge flow into the controller DC bus from the time of reset (Ah)
    /// </summary>
    public float DcBusAmpHrs { get; } = dcBusAmpHrs;

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

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out OdometerBusAmpHrs packet)
    {
		if (!IsValidId(id, isExtended) || data.Length < Size)
		{
			packet = default;
			return false;
		}

		// Hack to avoid extra range checks
		ReadOnlySpan<byte> a = MemoryMarshal.CreateReadOnlySpan(in data[0], Size);

		float odometer = BinaryPrimitives.ReadSingleLittleEndian(a);
        float dcBusAmpHrs = BinaryPrimitives.ReadSingleLittleEndian(a.Slice(sizeof(float)));

        packet = new OdometerBusAmpHrs(odometer, dcBusAmpHrs);
        return true;
    }
}
