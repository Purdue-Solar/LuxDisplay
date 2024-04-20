using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared
{
	public class Peripheral
	{
		public ushort Peripheral1_Status { get; set; }
		public ushort Peripheral2_Status { get; set; }
		public ushort Peripheral3_Status { get; set; }
		public ushort Peripheral4_Status { get; set; }

		public void PeripheralCan(uint device_src_id, uint message_id, byte[] data)
		{
			if (message_id == 0x0)//status
			{
				if (device_src_id == 0x50)
				{
					Peripheral1_Status = BitConverter.ToUInt16(data, 0);
				}
				if (device_src_id == 0x51)
				{
					Peripheral2_Status = BitConverter.ToUInt16(data, 0);
				}
				if (device_src_id == 0x52)
				{
					Peripheral3_Status = BitConverter.ToUInt16(data, 0);
				}
				if (device_src_id == 0x53)
				{
					Peripheral4_Status = BitConverter.ToUInt16(data, 0);
				}
			}
		}
	}
}
