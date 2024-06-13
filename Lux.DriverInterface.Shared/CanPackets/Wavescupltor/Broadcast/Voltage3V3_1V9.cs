﻿using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
public struct Voltage3V3_1V9(float voltage1v9, float voltage3v3) : IReadableCanPacket<Voltage3V3_1V9>
{
    public static uint CanId => WavesculptorBase.BroadcastBaseId + (uint)BroadcastId.Voltage3V3_1V9;
    public readonly uint Id => CanId;
    public static bool IsExtended => false;
    public static int Size => 8;

    /// <summary>
    /// Actual voltage of the 1.9V DSP power rail
    /// </summary>
    public float Voltage1V9 { get; set; } = voltage1v9;
    /// <summary>
    /// Actual voltage of the 3.3V power rail
    /// </summary>
    public float Voltage3V3 { get; set; } = voltage3v3;

    public static bool IsValidId(uint id, bool extended) => !extended && id == CanId;

	static bool IReadableCanPacket.TryRead(uint id, bool extended, ReadOnlySpan<byte> data, out IReadableCanPacket packet)
	{
		bool flag = TryRead(id, extended, data, out var genericPacket);
		packet = genericPacket;
		return flag;
	}

	public static bool TryRead(uint id, bool isExtended, ReadOnlySpan<byte> data, out Voltage3V3_1V9 packet)
    {
        packet = default;

        if (isExtended || id != CanId)
            return false;

        if (data.Length < Size)
            return false;

        float voltage1V9 = BinaryPrimitives.ReadSingleLittleEndian(data);
        float voltage3V3 = BinaryPrimitives.ReadSingleLittleEndian(data.Slice(sizeof(float)));

        packet = new Voltage3V3_1V9(voltage1V9, voltage3V3);
        return true;
    }

}