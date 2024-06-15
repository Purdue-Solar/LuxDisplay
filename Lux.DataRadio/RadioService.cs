using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Lux.DataRadio;

public class RadioService : BackgroundService
{
	protected IConfiguration Configuration { get; }
	protected SerialPort SerialPort { get; private set; } = new SerialPort();

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{

		////sets up the serial port
		//serialPort.PortName = "";
		//serialPort.BaudRate = 115200;
		//serialPort.DataBits = 8;
		//serialPort.StopBits = StopBits.One;
		//serialPort.Parity = Parity.None;

		//using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
		//int ValtoSend = 0;
		//try
		//{
		//	while (await timer.WaitForNextTickAsync(stoppingToken))
		//	{
		//		try
		//		{
		//			serialPort.Open();
		//		}
		//		catch (Exception ex)
		//		{
		//			//Error Opening Serial Port
		//		}
		//		serialPort.Write(ValtoSend.ToString("x2")); //converts to 8 bit hex string
		//		serialPort.Close();
		//	}
		//}
		//catch (OperationCanceledException)
		//{
		//	//Error
		//}

	}
}
