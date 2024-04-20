using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared
{
	public class WaveSculptor
	{
		public WaveSculptorCritical Critical { get; set; } = new WaveSculptorCritical();
		public WaveSculptorRaw Raw { get; set; } = new WaveSculptorRaw();
		public int Error_Flags_MC { get; set; }
		public int Limit_Flags_MC { get; set; }
		public double Bus_Current { get; set; }
		public double Bus_Voltage { get; set; }
		public double Vehicle_Velocity { get; set; }
		public double Motor_Velocity { get; set; }
		public double Phase_C_Current { get; set; }
		public double Phase_B_Current { get; set; }
		public double Vd_Real { get; set; }
		public double Vq_Imaginary { get; set; }
		public double Id_Real { get; set; }
		public double Iq_Imaginary { get; set; }
		public double BEMFd { get; set; }
		public double BEMFq { get; set; }
		public double _15V_Supply { get; set; }
		public double Heat_Sink_Temp { get; set; }
		public double Motor_Temp { get; set; }
		public double DSP_Board_Temp { get; set; }
		public double DC_Bus_Amp_Hours { get; set; }
		public double Odometer { get; set; }
		public double Slip_Speed { get; set; }

		public record WaveSculptorCritical
		{
			public double Bus_Current { get; set; }
			public double Bus_Voltage { get; set; }
			public double Vehicle_Velocity { get; set; }
			public double Motor_Velocity { get; set; }
			public double Heat_Sink_Temp { get; set; }
			public double Motor_Temp { get; set; }
		}

		public class WaveSculptorRaw
		{
			public ulong MotorPowerCommand { get; set; }
			public ulong IdentificationInformation { get; set; }
			public ulong StatusInformation { get; set; }
			public ulong BusMeasurement { get; set; }
			public ulong VelocityMeasurement { get; set; }
			public ulong PhaseCurrentMeasurement { get; set; }
			public ulong MotorVoltageVectorMeasurement { get; set; }
			public ulong MotorCurrentVectorMeasurement { get; set; }
			public ulong MotorBackEMFMeasurementPred { get; set; }
			public ulong VoltageRailMeasurement1 { get; set; }
			public ulong VoltageRailMeasurement2 { get; set; }
			public ulong FanSpeedMeasurement { get; set; }
			public ulong SinkMotorTemperatureMeasurement { get; set; }
			public ulong AirInCPUTemperatureMeasurement { get; set; }
			public ulong AirOutCapTemperatureMeasurement { get; set; }
			public ulong OdometerBusAmpHoursMeasurement { get; set; }
		}
	}
}
