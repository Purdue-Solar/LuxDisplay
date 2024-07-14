using Lux.DriverInterface.Shared;
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
	
	protected float MaxVoltage { get; } = config.GetValue($"{nameof(CanAutoSender)}:Mppts:{nameof(MaxVoltage)}", 112.0f);

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		new Thread(ExecuteThread).Start(stoppingToken);

		return Task.CompletedTask;
	}


	private void ExecuteThread(object? state)
	{
		if (state is not CancellationToken token)
			return;

		//using Timer mpptMaxVoltageTimer = new Timer(SendMpptMaxVoltage, null, 0, 1000);

		while (!token.IsCancellationRequested)
		{
			Thread.Sleep(100);
		}
	}

	private void SendMpptMaxVoltage(object? state)
	{
		for (int i = 0; i < Mppts.Count; i++)
		{
			DriverInterface.Shared.CanPackets.Elmar.Command.MaxOutputVoltage packet = new Lux.DriverInterface.Shared.CanPackets.Elmar.Command.MaxOutputVoltage(Mppts[(byte)i].DeviceId + DriverInterface.Shared.CanPackets.Elmar.ElmarBase.BaseId, MaxVoltage);

			ServiceBase.Write(packet.ToCanFrame());
		}
	}
}
