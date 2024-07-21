
using static Lux.DriverInterface.Shared.CanPackets.Battery.Status1;
using static Lux.DriverInterface.Shared.CanPackets.Battery.Status2;

namespace Lux.DriverInterface.Shared;

public class Battery : IWarningGenerator
{
	public float Current { get; set; }
	public float Voltage { get; set; }
	public float PackPower { get; set; }
	public float CalculatedPower => Voltage * Current;

	public float StateOfCharge { get; set; }
	public RelayState RelayState { get; set; }
	public FailsafeStatus FailsafeStatus { get; set; }
	public CurrentLimitStatus CurrentLimits { get; set; }
	public float PackDCL { get; set; }
	public float PackCCL { get; set; }

	public float PackAmpHours { get; set; }
	public float AdaptivePackAmpHours { get; set; }

	public float LowCellVoltage { get; set; }
	public float HighCellVoltage { get; set; }
	public int LowVoltageCellId { get; set; }
	public int HighVoltageCellId { get; set; }

	public float AverageTemperature { get; set; }
	public float LowTemperature { get; set; }
	public float HighTemperature { get; set; }
	public int LowTemperatureId { get; set; }
	public int HighTemperatureId { get; set; }

	public const float CellWarningTemperature = 40;
	public const float CellCriticalTemperature = 50;
	public const float AmpHoursWarning = 10;
	public const float AmpHoursCritical = 5;
	public const float CurrentWarning = 20;
	public const float CurrentCritical = 23;

	public List<Warning> GetWarnings()
	{
		List<Warning> warnings = [];
		if (HighTemperature >= CellWarningTemperature && HighTemperature < CellCriticalTemperature)
			warnings.Add(new Warning(WarningType.Warning, $"Warning: High Cell Temperature: {HighTemperature}"));
		if (HighTemperature >= CellCriticalTemperature)
			warnings.Add(new Warning(WarningType.Critical, $"Critical: High Cell Temperature {HighTemperature}"));
		if (!RelayState.HasFlag(RelayState.ChargeEnable))
			warnings.Add(new Warning(WarningType.Warning, "Warning: Charge Disabled"));
		if (!RelayState.HasFlag(RelayState.DischargeEnable))
			warnings.Add(new Warning(WarningType.Critical, "Warning: Discharge Disabled"));
		if (AdaptivePackAmpHours < AmpHoursWarning && AdaptivePackAmpHours >= AmpHoursCritical)
			warnings.Add(new Warning(WarningType.Warning, "Warning: Low Battery"));
		if (AdaptivePackAmpHours < AmpHoursCritical)
			warnings.Add(new Warning(WarningType.Critical, "Critical: Low Battery"));
		if (Current >= CurrentWarning && Current < CurrentCritical)
			warnings.Add(new Warning(WarningType.Warning, $"Warning: High Current ({Current:N1}A > {(int)CurrentWarning})"));
		if (Current >= CurrentCritical)
			warnings.Add(new Warning(WarningType.Critical, $"Critical: High Current ({Current:N1}A) > {(int)CurrentCritical}"));

		return warnings;
	}
}
