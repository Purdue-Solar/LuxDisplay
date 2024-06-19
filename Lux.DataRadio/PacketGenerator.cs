using Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
using Microsoft.Extensions.Hosting;
using SocketCANSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WaveSculptorStatus = Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast.Status;
using SteeringStatus = Lux.DriverInterface.Shared.CanPackets.Steering.Status;
using Lux.DriverInterface.Shared;

namespace Lux.DataRadio;
public interface IPacketQueue
{
	void Enqueue(CanFrame frame);
	bool TryDequeue(out CanFrame frame);
}

public class PacketQueue : IPacketQueue
{
	protected ConcurrentQueue<CanFrame> Frames { get; } = new();

	public void Enqueue(CanFrame frame) => Frames.Enqueue(frame);

	public bool TryDequeue(out CanFrame frame) => Frames.TryDequeue(out frame);
}

public class PacketGeneratorService(IPacketQueue queue) : BackgroundService
{
	private IPacketQueue Queue { get; } = queue;

	private void GenerateWSStatusPacket(object? state)
	{
		if (state is not Random rand)
			return;
		WaveSculptorStatus wsStatus = new WaveSculptorStatus((WaveSculptorStatus.LimitFlags)rand.Next(), (WaveSculptorStatus.ErrorFlags)rand.Next(), 0, 0, 0);
		Queue.Enqueue(wsStatus.ToCanFrame());
	}

	private void GenerateWSBusMeasurement(object? state)
	{
		if (state is not Random rand)
			return;

		BusMeasurement packet = new BusMeasurement(rand.NextFloat(96f, 121.8f), rand.NextFloat(10f, 12f));
		Queue.Enqueue(packet.ToCanFrame());
	}

	private void GenerateWSVelocity(object? state)
	{
		if (state is not Random rand)
			return;

		float rpm = rand.NextFloat(800, 900);
		float speed = rpm / 60 * 0.53f * MathF.PI;
		VelocityMeasurement packet = new VelocityMeasurement(rpm, speed);
		Queue.Enqueue(packet.ToCanFrame());
	}

	private void GenerateSteeringPacket(object? state)
	{
		if (state is not Random rand)
			return;

		PsrCanId id = new PsrCanId(PsrCanId.MulticastDestination, CanIds.SteeringBase, (byte)DriverInterface.Shared.CanPackets.Steering.MessageId.Status, CanIds.DeviceType.Steering, CanIds.MessagePriority.Normal);
		SteeringStatus packet = new SteeringStatus(id.ToInteger(), (SteeringStatus.ButtonFlags)rand.Next(), (byte)rand.Next(0, 8), 0, rand.NextFloat(17, 20));
		Queue.Enqueue(packet.ToCanFrame());
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using Timer wsStatusTimer = new Timer(GenerateWSStatusPacket, new Random(), TimeSpan.Zero, TimeSpan.FromMilliseconds(200));
		using Timer wsBusMeasurement = new Timer(GenerateWSBusMeasurement, new Random(), TimeSpan.FromMilliseconds(2), TimeSpan.FromMilliseconds(200));
		using Timer wsVelocity = new Timer(GenerateWSVelocity, new Random(), TimeSpan.FromMilliseconds(4), TimeSpan.FromMilliseconds(200));

		using Timer steering = new Timer(GenerateSteeringPacket, new Random(), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(100));
		
		await Task.Delay(-1, stoppingToken);
	}
}

internal static class RandExtensions
{
	public static double NextDouble(this Random rand, double min, double max)
	{
		return rand.NextDouble() * (max - min) + min;
	}

	public static float NextFloat(this Random rand, float min, float max)
	{
		return (float)(rand.NextDouble() * (max - min) + min);
	}
}