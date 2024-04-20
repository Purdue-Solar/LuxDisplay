using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Hosting;

namespace Lux.DataRadio
{
	public class RadioService : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			SerialPort _serialPort = new SerialPort();

			//sets up the serial port
			_serialPort.PortName = "COM3";
			_serialPort.BaudRate = 115200;
			_serialPort.DataBits = 8;
			_serialPort.StopBits = StopBits.One;
			_serialPort.Parity = Parity.None;

			using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
			int ValtoSend = 0;
			try
			{
				while (await timer.WaitForNextTickAsync(stoppingToken))
				{
					try
					{
						_serialPort.Open();
					}
					catch (Exception ex)
					{
						//Error Opening Serial Port
					}
					_serialPort.Write(ValtoSend.ToString("x2")); //converts to 8 bit hex string
					_serialPort.Close();
				}
			}
			catch (OperationCanceledException)
			{
				//Error
			}

		}
	}
}
