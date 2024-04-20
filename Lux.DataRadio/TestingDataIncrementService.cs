using Microsoft.Extensions.Hosting;
using System.Reflection.PortableExecutable;
using Lux.DriverInterface.Shared;

namespace Lux.DataRadio
{
	public class TestingDataIncrementService(WaveSculptor wsc, Header header, EMU emu) : BackgroundService
	{
		private readonly WaveSculptor _wsc = wsc;
		private readonly Header _header = header;
		private readonly EMU _emu = emu;
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using PeriodicTimer timer = new(TimeSpan.FromSeconds(.01));
			int x = 0;
			try
			{
				while (await timer.WaitForNextTickAsync(stoppingToken))
				{
					//Changing Header Values
					_header.BlinkerToggleLeft = true;
					_header.BlinkerToggleRight = true;

					//demonstrates how to toggle the hazard light
					if (x % 100 == 0)
					{
						_header.Hazard = !_header.Hazard;
						_header.Overheat = !_header.Overheat;
						_header.HeartbeatFailure = !_header.HeartbeatFailure;
						_header.Headlights = !_header.Headlights;
					}
					//Changing Motor Values
					_wsc.Critical.Vehicle_Velocity = _wsc.Critical.Vehicle_Velocity + 1;
					_wsc.Critical.Vehicle_Velocity %= 100;
					_wsc.Critical.Motor_Velocity = _wsc.Critical.Motor_Velocity + 1;
					_wsc.Critical.Motor_Velocity %= 1000;
					_wsc.Critical.Bus_Current = _wsc.Critical.Bus_Current + 1;
					_wsc.Critical.Bus_Current %= 50;
					_wsc.Critical.Bus_Voltage = _wsc.Critical.Bus_Voltage + 1;
					_wsc.Critical.Bus_Voltage %= 120;
					_wsc.Critical.Heat_Sink_Temp = _wsc.Critical.Heat_Sink_Temp + 1;
					_wsc.Critical.Heat_Sink_Temp %= 100;
					_wsc.Critical.Motor_Temp = _wsc.Critical.Motor_Temp + 1;
					_wsc.Critical.Motor_Temp %= 100;


					//Changing Battery Values
					_emu.Critical.Total_Charge_Energy = _emu.Critical.Total_Charge_Energy + 1;
					_emu.Critical.Total_Charge_Energy %= 1000;
					_emu.Critical.Total_Discharge_Energy = _emu.Critical.Total_Discharge_Energy + 1;
					_emu.Critical.Total_Discharge_Energy %= 1000;
					_emu.Critical.Min_Pack_Voltage = _emu.Critical.Min_Pack_Voltage + 1;
					_emu.Critical.Min_Pack_Voltage %= 100;
					_emu.Critical.Max_Pack_Voltage = _emu.Critical.Max_Pack_Voltage + 1;
					_emu.Critical.Max_Pack_Voltage %= 100;
					_emu.Critical.Min_Cell_Temp = _emu.Critical.Min_Cell_Temp + 1;
					_emu.Critical.Min_Cell_Temp %= 100;
					_emu.Critical.Max_Cell_Temp = _emu.Critical.Max_Cell_Temp + 1;
					_emu.Critical.Max_Cell_Temp %= 100;

					x++;
				}
			}
			catch (OperationCanceledException)
			{
				//Error
			}
		}

	}
}
