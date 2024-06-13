using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
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

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out OdometerBusAmpHrs packet)
    {
        packet = default;
        if (extended || id != CanId || data.Length < Size)
        {
            return false;
        }

        float odometer = BinaryPrimitives.ReadSingleLittleEndian(data);
        float dcBusAmpHrs = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new OdometerBusAmpHrs(odometer, dcBusAmpHrs);
        return true;
    }
}
