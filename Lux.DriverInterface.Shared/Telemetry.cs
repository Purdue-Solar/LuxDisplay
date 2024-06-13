using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared
{
	public class Telemetry
	{
		public short XAccelorometer { get; set; }
		public short YAccelorometer { get; set; }
		public short ZAccelorometer { get; set; }
		public short XCompass { get; set; }
		public short YCompass { get; set; }
		public short ZCompass { get; set; }
		public short XGyro { get; set; }
		public short YGyro { get; set; }
		public short ZGyro { get; set; }
		public short Temp { get; set; }
		public bool PressureSensor1 { get; set; }
		public bool PressureSensor2 { get; set; }
		public int CabinTemperature { get; set; }
		public int CabinHumiditiy { get; set; }
	}
}
