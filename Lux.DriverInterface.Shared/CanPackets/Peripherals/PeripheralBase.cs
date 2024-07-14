using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Peripherals;

public enum MessageId : byte
{
	Status = 0x00,
	ChangeOutputs = 0x01
}

public static class PeripheralBase
{
	public const int MaxPeripheralCount = 8;
}
