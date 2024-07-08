using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public class Distribution
{
	public CanPackets.Distribution.Status.StatusFlags Flags { get; set; }

	public short RawMainVoltage { get; set; }
	public short RawAuxVoltage { get; set; }
	public float VoltageScaleFactor { get; set; }
	[JsonIgnore]
	public float MainVoltage => RawMainVoltage * VoltageScaleFactor;
	[JsonIgnore]
	public float AuxVoltage => RawAuxVoltage * VoltageScaleFactor;

	public short RawMainCurrent { get; set; }
	public short RawAuxCurrent { get; set; }
	public float CurrentScaleFactor { get; set; }
	[JsonIgnore]
	public float MainCurrent => RawMainCurrent * CurrentScaleFactor;
	[JsonIgnore]
	public float AuxCurrent => RawAuxCurrent * CurrentScaleFactor;

	public short RawMainTemperature { get; set; }
	public short RawAuxTemperature { get; set; }
	public float TemperatureScaleFactor { get; set; }
	[JsonIgnore]
	public float MainTemperature => RawMainTemperature * TemperatureScaleFactor;
	[JsonIgnore]
	public float AuxTemperature => RawAuxTemperature * TemperatureScaleFactor;

	public ushort RawMainPower { get; set; }
	public ushort RawAuxPower { get; set; }
	public float PowerScaleFactor { get; set; }
	[JsonIgnore]
	public float MainPower => RawMainPower * PowerScaleFactor;
	[JsonIgnore]
	public float AuxPower => RawAuxPower * PowerScaleFactor;

	public uint RawMainEnergy { get; set; }
	public uint RawAuxEnergy { get; set; }
	public float EnergyScaleFactor { get; set; }
	[JsonIgnore]
	public float MainEnergy => RawMainEnergy * EnergyScaleFactor;
	[JsonIgnore]
	public float AuxEnergy => RawAuxEnergy * EnergyScaleFactor;

}
