using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocketCANSharp;

namespace Lux.DataRadio
{
	public class Shared
	{
		public static ConcurrentQueue<CanFrame> CANQueue = new ConcurrentQueue<CanFrame>();
	}
}
