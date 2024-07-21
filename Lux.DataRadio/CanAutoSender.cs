using Lux.DriverInterface.Shared;
using Lux.DriverInterface.Shared.CanPackets.Elmar;
using Lux.DriverInterface.Shared.CanPackets.Elmar.Command;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DataRadio;
public class CanAutoSender(IConfiguration config, ICanServiceBase serviceBase, MpptCollection mppts) : BackgroundService
{
	public ICanServiceBase ServiceBase { get; } = serviceBase;
	public MpptCollection Mppts { get; } = mppts;
	
	protected float MaxVoltage0 { get; } = config.GetValue($"{nameof(CanAutoSender)}:Mppts:{nameof(MaxVoltage0)}", 113.0f);
	protected float MaxVoltage1 { get; } = config.GetValue($"{nameof(CanAutoSender)}:Mppts:{nameof(MaxVoltage1)}", 112.8f);
	protected float MaxVoltage2 { get; } = config.GetValue($"{nameof(CanAutoSender)}:Mppts:{nameof(MaxVoltage2)}", 112.6f);

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		new Thread(ExecuteThread).Start(stoppingToken);

		return Task.CompletedTask;
	}


	private void ExecuteThread(object? state)
	{
		if (state is not CancellationToken token)
			return;

		using Timer mpptMaxVoltageTimer = new Timer(SendMpptMaxVoltage, null, 0, 1000);

		while (!token.IsCancellationRequested)
		{
			Thread.Sleep(100);
		}
	}

	private void SendMpptMaxVoltage(object? state)
	{
		float[] values = [ MaxVoltage0, MaxVoltage1, MaxVoltage2 ];
		for (int i = 0; i < Mppts.Count; i++)
		{
			MaxOutputVoltage packet = new MaxOutputVoltage((uint)((Mppts[(byte)i].DeviceId << 4) + ElmarBase.BaseId + (byte)CommandId.MaxOutputVoltage), values[i]);

			ServiceBase.Write(packet.ToCanFrame());
		}
	}
}
