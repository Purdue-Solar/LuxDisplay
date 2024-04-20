using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared
{
	public class EMU
	{
		public EMURaw Raw { get; set; } = new EMURaw();
		public EMUCritical Critical { get; set; } = new EMUCritical();
		public double Total_Discharge { get; set; }
		public double Total_Charge { get; set; }
		public double Total_Discharge_Energy { get; set; }
		public double Total_Charge_Energy { get; set; }
		public double Total_Discharge_Time { get; set; }
		public double Total_Charge_Time { get; set; }
		public double Total_Distance { get; set; }
		public double Max_Discharge_Current { get; set; }
		public double Max_Charge_Current { get; set; }
		public int Master_Clear_Count { get; set; }
		public double Min_Cell_Voltage { get; set; }
		public double Max_Cell_Voltage { get; set; }
		public double Max_Cell_Voltag_Diff { get; set; }
		public double Min_Pack_Voltage { get; set; }
		public double Max_Pack_Voltage { get; set; }
		public double Min_Cell_Mod_Temp { get; set; }
		public double Max_Cell_Mod_Temp { get; set; }
		public double Max_Cell_Module_Temp_Diff { get; set; }
		public int Bms_Start_Count { get; set; }
		public int Under_Volt_Prot_Cnt { get; set; }
		public int Over_Volt_Prot_Cnt { get; set; }
		public int Discharge_Over_Current_Prot_Cnt { get; set; }
		public int Charge_Over_Current_Prot_Cnt { get; set; }
		public int Cell_Mod_Overheat_Prot_Cnt { get; set; }
		public int Leakage_Prot_Cnt { get; set; }
		public int No_Cell_Comm_Prot_Cnt { get; set; }
		public int Low_Volt_Power_Reduction_Cnt { get; set; }
		public int High_Current_Power_Reduction_Cnt { get; set; }
		public int High_Cell_Mod_Temp_Power_Reduction_Cnt { get; set; }
		public int Charger_Connect_Cnt { get; set; }
		public int Charger_Disconnect_Cnt { get; set; }
		public int Pre_heat_Stage_Cnt { get; set; }
		public int Pre_charge_Stage_Cnt { get; set; }
		public int Main_Charge_Stage_Count { get; set; }
		public int Balancing_Stage_Cnt { get; set; }
		public int Charging_Finished_Cnt { get; set; }
		public bool Charging_Error_Occurred { get; set; }
		public int Charging_Retry_Count { get; set; }
		public int Trips_Count { get; set; }
		public int Charge_Restarts_Cnt { get; set; }
		public int Cell_Overheat_Prot_Cnt { get; set; }
		public int High_Cell_Temp_Power_Reduction_Cnt { get; set; }
		public double Min_Cell_Temp { get; set; }
		public double Max_Cell_Temp { get; set; }
		public double Max_Cell_Temp_Diff { get; set; }
		public int Secure_Stats_Resolution { get; set; }
		public int No_Current_Sensor_Prot_Cnt { get; set; }
		public int Heater_Activation_Count { get; set; }
		public record EMURaw
		{
			public ulong OverallParameters { get; set; }
			public ulong DiagnosticCodes { get; set; }
			public ulong BatteryVoltageOverallParameters { get; set; }
			public ulong CellModuleTempOverallParameters { get; set; }
			public ulong CellTempOverallParameters { get; set; }
			public ulong CellBalancingRateOverallParameters { get; set; }
		}

		public record EMUCritical
		{
			public double Total_Discharge_Energy { get; set; }
			public double Total_Charge_Energy { get; set; }
			public double Min_Pack_Voltage { get; set; }
			public double Max_Pack_Voltage { get; set; }
			public double Min_Cell_Temp { get; set; }
			public double Max_Cell_Temp { get; set; }
		}
	}
}
