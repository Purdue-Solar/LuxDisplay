using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast.Status;

namespace Lux.DriverInterface.Shared;
public class PeripheralCollection
{
	public Peripheral[] Peripherals { get; } = new Peripheral[CanPackets.Peripherals.PeripheralBase.MaxPeripheralCount];
	public int Count => Peripherals.Length;

	public PeripheralCollection()
	{
		for (byte i = 0; i < Peripherals.Length; i++)
		{
			Peripherals[i] = new Peripheral(i);
		}
	}

	public Peripheral this[byte deviceId]
	{
		get
		{
			if (deviceId >= Peripherals.Length)
				throw new ArgumentOutOfRangeException(nameof(deviceId));

			return Peripherals[deviceId];
		}
	}
}
