using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using SocketCANSharp;
using SocketCANSharp.Network;

namespace Lux.DataRadio
{

	public class CANReceiveService(WaveSculptor wsc, Header header, EMU emu, Telemetry telemetry, Peripheral peripheral) : BackgroundService
	{
		private readonly WaveSculptor _wsc = wsc;
		private readonly Header _header = header;
		private readonly EMU _emu = emu;
		private readonly Telemetry _telemetry = telemetry;
		private readonly Peripheral _peripheral = peripheral;
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			int bytesRead = 1;
			CanFrame frame = new CanFrame();
			using (var rawCanSocket = new RawCanSocket())
			{
				CanNetworkInterface can0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("can0"));
				rawCanSocket.Bind(can0);
				while (!stoppingToken.IsCancellationRequested)
				{
					//Reading from CAN
					bytesRead = await Task.Run(() => rawCanSocket.Read(out frame));
					if (bytesRead != 0)
					{
						uint category = (frame.CanId & 0x07C00000) >> 22;
						uint src_device = (frame.CanId & 0x0000FF00) >> 8;
						uint message_id = (frame.CanId & 0x003F0000) >> 16;
						//Telemtry board
						if ((int)category == 7)
						{
							_telemetry.Telemetry_Can_General(frame.Data);
						}

						//Peripheral Controllers
						if ((int)category == 5)
						{
							_peripheral.PeripheralCan(src_device, message_id, frame.Data);
						}
						if (frame.CanId >> 31 == 1)//check for extended can frame
						{

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
