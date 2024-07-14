using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;

public static class CanIds
{
	public const byte MpptBase = (byte)DeviceType.Mppt << 4;
	public const byte BmsBase = (byte)DeviceType.Bms << 4;
	public const byte MotorControllerBase = (byte)DeviceType.MotorController << 4;
	public const byte DisplayBase = (byte)DeviceType.Display << 4;
	public const byte DistributionBase = (byte)DeviceType.Distribution << 4;
	public const byte PeripheralsBase = (byte)DeviceType.Peripherals << 4;
	public const byte SteeringBase = (byte)DeviceType.Steering << 4;
    public const byte TelemetryBase = (byte)DeviceType.Telemetry << 4;

    /// <summary>
    /// 5 bit Device Type
    /// </summary>
    public enum DeviceType : byte
	{
		Mppt = 0x0,
		Bms = 0x1,
		MotorController = 0x2,
		Display = 0x3,
		Distribution = 0x4,
		Peripherals = 0x5,
		Steering = 0x6,
		Telemetry = 0x7,

		Generic = 0x1F
	}

	/// <summary>
	/// Priority of the message
	/// </summary>
	public enum MessagePriority : byte
	{
		Highest = 0x00,
		High = 0x01,
		Normal = 0x02,
		Low = 0x03,
	}
	

	public static class GenericMessage
	{
		/// <summary>
		/// Generic message rate in milliseconds
		/// </summary>
		public const int GenericRate = 2000;

		// Generic Message IDs
		public const byte Heartbeat = 0x00;
		public const byte VoltageCurrent0 = 0x01;
		public const byte VoltageCurrent1 = 0x02;
		public const byte VoltageCurrent2 = 0x03;
		public const byte VoltageCurrent3 = 0x04;

		public const byte Errors0 = 0x20;
		public const byte Errors1 = 0x21;
		public const byte Errors2 = 0x22;
		public const byte Errors3 = 0x23;

		public const byte Reset = 0x3F;
    }
}
