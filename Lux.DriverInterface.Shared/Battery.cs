
using Lux.DriverInterface.Shared.CanPackets.Battery;
using static Lux.DriverInterface.Shared.CanPackets.Battery.Status1;
using static Lux.DriverInterface.Shared.CanPackets.Battery.Status2;

namespace Lux.DriverInterface.Shared;

public class Battery : IWarningGenerator
{
	public float Current { get; set; }
	public float Voltage { get; set; }
	public float PackPower { get; set; }
	public float CalculatedPower => Voltage * Current;

	public float CalculatedStateOfCharge { get; set; }

	public float StateOfCharge { get; set; }
	public RelayState RelayState { get; set; }
	public FailsafeStatus FailsafeStatus { get; set; }
	public CurrentLimitStatus CurrentLimits { get; set; }
	public float PackDCL { get; set; }
	public float PackCCL { get; set; }

	public float PackAmpHours { get; set; }
	public float AdaptivePackAmpHours { get; set; }

	public PackAmpHoursFaults.DtcCodes FaultCodes { get; set; }

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

	public const float LowCellWaring = 2.7f;
	public const float LowCellCritical = 2.5f;

	public const float HighCellWarning = 4.17f;
	public const float HighCellCritical = 4.2f;

	public List<Warning> GetWarnings()
	{
		List<Warning> warnings = [];


		if (FaultCodes != 0)
		{
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.DischargeLimitEnforcementFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Discharge Limit Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.InternalHardwareFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Battery Hardware Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.InternalSoftwareFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Battery Software Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.HighestCellVoltageTooHighFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: High Cell Voltage Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.LowestCellVoltageTooLowFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Low Cell Voltage Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.PackTooHotFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Battery Too Hot Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.CellBalancingStuckOffFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Cell Balancing Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.WeakCellFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Weak Cell Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.OpenWiringFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Open Wiring Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.CurrentSensor))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Current Sensor Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.HighestCellVoltageOver5VFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: High Cell Voltage Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.CellAsicFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Cell ASIC Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.WeakPackFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Weak Pack Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.ThermistorFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Battery Thermistor Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.HighVoltageIsolationFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: High Voltage Isolation Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.InputPowerSupplyFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Input Power Supply Fault"));
			if (FaultCodes.HasFlag(PackAmpHoursFaults.DtcCodes.ChargeLimitEnforcementFault))
				warnings.Add(new Warning(WarningType.Critical, "Critical: Charge Limit Fault"));
		}

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

		if (CurrentLimits != 0)
		{
			if (CurrentLimits.HasFlag(CurrentLimitStatus.DclReducedLowSoc))
				warnings.Add(new Warning(WarningType.Warning, $"DCL Reduced: Low SOC ({PackDCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.DclReducedHighResistance))
				warnings.Add(new Warning(WarningType.Warning, $"DCL Reduced: High Resistance ({PackDCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.DclReducedTemperature))
				warnings.Add(new Warning(WarningType.Warning, $"DCL Reduced: High Temperature ({PackDCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.DclReducedLowPackVoltage))
				warnings.Add(new Warning(WarningType.Warning, $"DCL Reduced: Low Pack Voltage ({PackDCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.DclReducedLowCellVoltage))
				warnings.Add(new Warning(WarningType.Warning, $"DCL Reduced: Low Cell Voltage ({PackDCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.CclReducedHighSoc))
				warnings.Add(new Warning(WarningType.Warning, $"CCL Reduced: High SOC ({PackCCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.CclReducedHighResistance))
				warnings.Add(new Warning(WarningType.Warning, $"CCL Reduced: High Resistance ({PackCCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.CclReducedTemperature))
				warnings.Add(new Warning(WarningType.Warning, $"CCL Reduced: High Temperature ({PackCCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.CclReducedHighCellVoltage))
				warnings.Add(new Warning(WarningType.Warning, $"CCL Reduced: High Cell Voltage ({PackCCL}A)"));
			if (CurrentLimits.HasFlag(CurrentLimitStatus.CclReducedHighPackVoltage))
				warnings.Add(new Warning(WarningType.Warning, $"CCL Reduced: High Pack Voltage ({PackCCL}A)"));
		}

		return warnings;
	}
}
