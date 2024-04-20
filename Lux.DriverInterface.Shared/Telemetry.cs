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
		public short temp { get; set; }
		public short PressureSensor1 { get; set; }
		public short PressureSensor2 { get; set; }
		//public short DHT_Temp { get; set; }
		public int DHT_Temp { get; set; }
		//public short DHT_Humidity { get; set; }
		public int DHT_Humidity { get; set; }



		public void Telemetry_Can_General(byte[] bytes)
		{
			/*
            XAccelorometer = (short)BitConverter.ToInt16(bytes, 0);
            YAccelorometer = (short)BitConverter.ToInt16(bytes, 2);
            ZAccelorometer = (short)BitConverter.ToInt16(bytes, 4);
            XCompass = (short)BitConverter.ToInt16(bytes, 6);
            YCompass = (short)BitConverter.ToInt16(bytes, 8);
            ZCompass = (short)BitConverter.ToInt16(bytes, 10);
            XGyro = (short)BitConverter.ToInt16(bytes, 12);
            YGyro = (short)BitConverter.ToInt16(bytes, 14);
            ZGyro = (short)BitConverter.ToInt16(bytes, 16);
            temp = (short)BitConverter.ToInt16(bytes, 18);
            PressureSensor1 = (short)BitConverter.ToInt16(bytes, 20);
            PressureSensor2 = (short)BitConverter.ToInt16(bytes, 22);
            DHT_Temp = (short)BitConverter.ToInt16(bytes, 24);
            DHT_Humidity = (short)BitConverter.ToInt16(bytes, 26);
            */

			DHT_Humidity = BitConverter.ToInt32(bytes, 0);
			DHT_Temp = BitConverter.ToInt32(bytes, 4);
		}
	}
}
