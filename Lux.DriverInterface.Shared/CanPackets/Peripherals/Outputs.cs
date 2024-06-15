using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Peripherals;

public enum OutputState : byte
{
	Off = 0x00,
	On = 0x01,
	Blink = 0x02,
	NoChange = 0x03
}

public struct Outputs(ushort value)
{
	public ushort Value { get; private set; } = value;

	private const int OutputCount = 8;

	public OutputState this[int index]
	{
		readonly get
		{
			if (index < 0 || index >= OutputCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			int shift = index * 2;

			return (OutputState)((Value >> shift) & 0x03);
		}
		set
		{
			if (index < 0 || index >= OutputCount)
				throw new ArgumentOutOfRangeException(nameof(index));

			int shift = index * 2;
			ushort mask = (ushort)(0x03 << shift);
			
			Value &= (ushort)~mask;
			Value |= (ushort)(((byte)value & 0x03) << shift);
		}
	}
}
