using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets.Wavescupltor;

public enum BroadcastId
{
	Identification = 0x0,
	Status = 0x1,
	BusMeasurement = 0x2,
	VelocityMeasurement = 0x3,
	PhaseCurrent = 0x4,
	VoltageVector = 0x5,
	CurrentVector = 0x6,
	BackEmf = 0x7,
	Voltage15V = 0x8,
	Voltage3V3_1V9 = 0x9,
	Reserved0A = 0xA,
	HeatSinkMotorTemp = 0xB,
	DspBoardTemp = 0xC,
	Reserved0D = 0xD,
	OdometerBusAmpHrs = 0xE,
	SlipSpeed = 0x17
}

public enum CommandId
{
	Drive = 0x1,
	Power = 0x2,
	Reset = 0x3,
	ChangeMotor = 0x12
}

public static class WavesculptorBase
{
	public const uint BroadcastBaseId = 0x400;
	public const uint CommandBaseId = 0x500;
}
