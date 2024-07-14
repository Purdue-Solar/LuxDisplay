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

	public override string ToString()
	{
		StringBuilder sb = new();
		sb.Append(DeviceId).Append(", ");
		sb.Append(InputVoltage).Append(", ");
		sb.Append(InputCurrent).Append(", ");
		sb.Append(OutputVoltage).Append(", ");
		sb.Append(OutputCurrent).Append(", ");
		sb.Append(MosfetTemperature).Append(", ");
		sb.Append(ControllerTemperature).Append(", ");
		sb.Append(Voltage12V).Append(", ");
		sb.Append(Voltage3V).Append(", ");
		sb.Append(MaxOutputVoltage).Append(", ");
		sb.Append(MaxInputCurrent).Append(", ");
		sb.Append(RxErrorCount).Append(", ");
		sb.Append(TxErrorCount).Append(", ");
		sb.Append(TxOverflowCount).Append(", ");
		sb.Append(ErrorFlags).Append(", ");
		sb.Append(LimitFlags).Append(", ");
		sb.Append(Mode).Append(", ");
		sb.Append(TestCounter).Append(", ");
		sb.Append(PowerConnectorVoltage).Append(", ");
		sb.Append(PowerConnectorTemp);
		return sb.ToString();
	}
}
