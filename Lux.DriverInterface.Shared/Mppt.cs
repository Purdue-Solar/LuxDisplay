using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public class Mppt(byte deviceId)
{
	public byte DeviceId { get; } = (byte)(deviceId & CanPackets.Elmar.ElmarBase.DeviceIdMaskShifted);

	public float InputVoltage { get; set; }
	public float InputCurrent { get; set; }
	public float OutputVoltage { get; set; }
	public float OutputCurrent { get; set; }
	public float MosfetTemperature { get; set; }
	public float ControllerTemperature { get; set; }
	public float Voltage12V { get; set; }
	public float Voltage3V { get; set; }
	public float MaxOutputVoltage { get; set; }
	public float MaxInputCurrent { get; set; }
	public byte RxErrorCount { get; set; }
	public byte TxErrorCount { get; set; }
	public byte TxOverflowCount { get; set; }
	public CanPackets.Elmar.Broadcast.Status.ErrorFlags ErrorFlags { get; set; }
	public CanPackets.Elmar.Broadcast.Status.LimitFlags LimitFlags { get; set; }
	/// <summary>
	/// MPPT mode (0 = standby, 1 = active)
	/// </summary>
	public byte Mode { get; set; }
	/// <summary>
	/// Test counter (incremented every second)
	/// </summary>
	public byte TestCounter { get; set; }
	public float PowerConnectorVoltage { get; set; }
	public float PowerConnectorTemp { get; set; }

}
