using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public class Telemetry
{
	public bool BrakesEngaged { get; set; }
	public bool TemperatureWarning { get; set; }
	public bool TemperatureCritical { get; set; }
	public byte BrakePressure1 { get; set; }
	public byte BrakePressure2 { get; set; }
	public byte CabinTemperature { get; set; }
	public byte CabinHumiditiy { get; set; }
	public float RealBrakePressure1 { get; set; }
	public float RealBrakePressure2 { get; set; }
}
