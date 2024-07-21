using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lux.DriverInterface.Shared.CanPackets.WaveSculptor;
using ErrorFlags = Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast.Status.ErrorFlags;
using LimitFlags = Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast.Status.LimitFlags;

namespace Lux.DriverInterface.Shared;
public class WaveSculptor : IWarningGenerator
{
	public const float MpsToMph = 2.23693629f;

	public ErrorFlags ErrorFlags { get; set; }
	public LimitFlags LimitFlags { get; set; }
	public float BusCurrent { get; set; }
	public float BusVoltage { get; set; }
	public float VehicleVelocity { get; set; }
	public float MotorVelocity { get; set; }
	public float PhaseCCurrent { get; set; }
	public float PhaseBCurrent { get; set; }
	public float Vd { get; set; }
	public float Vq { get; set; }
	public float CurrentD { get; set; }
	public float CurrentQ { get; set; }
	public float BemfD { get; set; }
	public float BemfQ { get; set; }
	public float Voltage15 { get; set; }
	public float Voltage1V9 { get; set; }
	public float Voltage3V3 { get; set; }
	public float HeatsinkTemp { get; set; }
	public float MotorTemp { get; set; }
	public float DspBoardTemp { get; set; }
	public float DcBusAmpHrs { get; set; }
	public float Odometer { get; set; }
	public float SlipSpeed { get; set; }

	public float InputPower => BusVoltage * BusCurrent;
	public float OutputPower
	{
		get
		{
			float pd = Vd * CurrentD;
			float pq = Vq * CurrentQ;

			return MathF.Sqrt(pd * pd + pq * pq);
		}
	}

	public const float WarningCurrent = 20;
	public const float CriticalCurrent = 24;

	public List<Warning> GetWarnings()
	{
		List<Warning> warnings = [];

		if (BusCurrent >= CriticalCurrent)
			warnings.Add(new Warning(WarningType.Critical, "Critical: High Motor Power"));
		if (InputPower >= WarningCurrent && InputPower < CriticalCurrent)
			warnings.Add(new Warning(WarningType.Warning, "Warning: High Motor Power"));
		if (ErrorFlags.HasFlag(ErrorFlags.DcBusOverVoltage))
			warnings.Add(new Warning(WarningType.Critical, "Critical: DC Bus Over Voltage"));
		if (ErrorFlags.HasFlag(ErrorFlags.BadPositionHallSequence))
			warnings.Add(new Warning(WarningType.Warning, "Warning: Bad Hall Sequence"));
		if (ErrorFlags.HasFlag(ErrorFlags.MotorOverSpeed))
			warnings.Add(new Warning(WarningType.Critical, "Critical: Motor Over Speed"));
		if (ErrorFlags.HasFlag(ErrorFlags.SoftwareOverCurrent))
			warnings.Add(new Warning(WarningType.Critical, "Critical: Motor Over Current"));
		if (ErrorFlags.HasFlag(ErrorFlags.DesaturationFault))
			warnings.Add(new Warning(WarningType.Critical, "Critical: Desaturation Fault"));

		return warnings;
	}
}
