using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SocketCANSharp;
using SocketCANSharp.Network;

namespace Lux.DataRadio
{
	public class CANSendService : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using (var rawCanSocket = new RawCanSocket())
			{
				CanNetworkInterface can0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("can0"));
				rawCanSocket.Bind(can0);
				while (!stoppingToken.IsCancellationRequested)
				{
					while (!Shared.CANQueue.IsEmpty)
					{
						CanFrame frame = new CanFrame();
						if (Shared.CANQueue.TryDequeue(out frame))
						{
							rawCanSocket.Write(frame);
						}
						else
						{
						}
					}
				}
			}
		}
	}
}
