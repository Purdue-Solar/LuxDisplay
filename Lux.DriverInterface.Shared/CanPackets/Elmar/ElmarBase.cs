using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Elmar;

public enum BroadcastId : byte
{
	InputMeasurements = 0x0,
	OutputMeasurements = 0x1,
	Temperature = 0x2,
	AuxiliaryPowerSupply = 0x3,
	Limits = 0x4,
	Status = 0x5,
	PowerConnector = 0x6
}

public enum CommandId : byte
{
	Mode = 0x8,
	MaxOutputVoltage = 0xA,
	MaxInputCurrent = 0xB
}

public static class ElmarBase
{
	public const uint BaseId = 0x600;
	public const uint BaseMessageMask = 0x70F;
	
	public const int DeviceIdOffset = 4;
	public const int DeviceIdSize = 4;
	public const int DeviceIdMaskShifted = 0x00F;
	public const uint DeviceIdMask = 0x0F0;

	public const int MaxMpptCount = 16;
}
