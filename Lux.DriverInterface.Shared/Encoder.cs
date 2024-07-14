using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared
{
	public class Encoder
	{
		public ushort Value { get; set; } = 0;
		public float Percentage { get; set; } = 0;
		public PedalState State { get; set; } = PedalState.Neutral;
		public ControlMode Mode { get; set; } = ControlMode.Speed; 

		public enum PedalState
		{
			Neutral,
			Forward,
			Reverse
		}

		public enum ControlMode
		{
			Torque,
			Speed
		}
	}
}
