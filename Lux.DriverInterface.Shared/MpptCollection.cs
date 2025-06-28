using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast.Status;

namespace Lux.DriverInterface.Shared;
public class MpptCollection : IWarningGenerator
{
	public Mppt[] Mppts { get; } = new Mppt[CanPackets.Elmar.ElmarBase.MaxMpptCount];
	public int Count => Mppts.Length;

	public MpptCollection()
	{
		for (byte i = 0; i < Mppts.Length; i++)
		{
			Mppts[i] = new Mppt(i);
		}
	}

	public Mppt this[byte deviceId]
	{
		get
		{
			if (deviceId >= Mppts.Length)
				throw new ArgumentOutOfRangeException(nameof(deviceId));

			return Mppts[deviceId];
		}
	}

	public float TotalInputPower => Mppts.Sum(mppt => mppt.InputVoltage * mppt.InputCurrent);
	public float TotalOutputPower => Mppts.Sum(mppt => mppt.OutputVoltage * mppt.OutputCurrent);

	public float AverageEffiecency => TotalInputPower == 0 ? 0 : TotalOutputPower / TotalInputPower;

	public ErrorFlags AggregateFlags => Mppts.Aggregate(ErrorFlags.None, (v, mppt) => v | mppt.ErrorFlags);

	public LimitFlags AggregateLimits => Mppts.Aggregate(LimitFlags.None, (v, mppt) => v | mppt.LimitFlags);

	private const float LowArrayVoltageWarning = 20.0f;

	public List<Warning> GetWarnings()
	{
		List<Warning> warnings = [];

		if (AggregateFlags != 0)
		{
			if (AggregateFlags.HasFlag(ErrorFlags.LowArrayPower))
				warnings.Add(new Warning(WarningType.Warning, "Warning: Low Array Power"));
			if (AggregateFlags.HasFlag(ErrorFlags.MosfetOverheat))
				warnings.Add(new Warning(WarningType.Critical, "Critical: MPPT MOSFET Overheat"));
			if (AggregateFlags.HasFlag(ErrorFlags.BatteryFull))
				warnings.Add(new Warning(WarningType.Warning, "Warning: MPPT Battery Full"));
			if (AggregateFlags.HasFlag(ErrorFlags.BatteryLow))
				warnings.Add(new Warning(WarningType.Warning, "Warning: MPPT Battery Low"));
			if (AggregateFlags.HasFlag(ErrorFlags.Undervoltage12V))
				warnings.Add(new Warning(WarningType.Critical, "Critical: MPPT Undervoltage"));
			if (AggregateFlags.HasFlag(ErrorFlags.HwOvercurrent))
				warnings.Add(new Warning(WarningType.Critical, "Critical: MPPT Overcurrent"));
			if (AggregateFlags.HasFlag(ErrorFlags.HwOvervoltage))
				warnings.Add(new Warning(WarningType.Critical, "Critical: MPPT Overvoltage"));
		}

		foreach(Mppt mppt in Mppts)
		{
			if (mppt.InputVoltage <= LowArrayVoltageWarning)
				warnings.Add(new Warning(WarningType.Warning, $"Warning: Low Array Voltage (Array {mppt.DeviceId})"));
		}

		return warnings;
	}
}
