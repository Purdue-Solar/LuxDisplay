using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DataRadio;
public class StateOfChargeService(IConfiguration config, Battery battery, WaveSculptor waveSculptor) : BackgroundService
{
	protected IConfiguration Configuration { get; } = config;
	protected Battery Battery { get; } = battery;
	protected WaveSculptor WaveSculptor = waveSculptor;

	private double PackResistance { get; } = config.GetValue($"StateOfChargeService:{nameof(PackResistance)}", 0.1);
	private double CellWattHours { get; } = config.GetValue($"StateOfChargeService:{nameof(CellWattHours)}", 18.0);
	private double Series { get; } = config.GetValue($"StateOfChargeService:{nameof(Series)}", 28);
	private double CurrentOffset { get; } = config.GetValue($"StateOfChargeService:{nameof(CurrentOffset)}", 0.2);

	private double[] Coefficients { get; } = config.GetSection($"StateOfChargeService:{nameof(Coefficients)}").Get<double[]>() ?? [];

	private double CalculateStateOfCharge(double voltage, double current)
	{
		double trueVoltage = voltage - (current - CurrentOffset) * PackResistance;
		double multiplier = trueVoltage / Series;
		
		double x = 1;
		double sum = 0;

		for (int i = 0; i < Coefficients.Length; i++)
		{
			sum += Coefficients[i] * Math.Pow(multiplier, i);
		}

		double wh = CellWattHours - sum;

		return wh / CellWattHours;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_ = Task.Run(() => ExecuteThread(stoppingToken), stoppingToken);
		return Task.CompletedTask;
	}

	private async Task ExecuteThread(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			Battery.CalculatedStateOfCharge = (float)CalculateStateOfCharge(WaveSculptor.BusVoltage, -Battery.Current);
			await Task.Delay(500, stoppingToken);
		}
	}
}
