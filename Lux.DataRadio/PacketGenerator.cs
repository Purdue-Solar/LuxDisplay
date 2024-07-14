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
using DistributionStatus = Lux.DriverInterface.Shared.CanPackets.Distribution.Status;
using Lux.DriverInterface.Shared;
using System.Runtime.InteropServices.ObjectiveC;
using Lux.DriverInterface.Shared.CanPackets.Distribution;
using Lux.DriverInterface.Shared.CanPackets.Elmar;
using Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;

namespace Lux.DataRadio;
public interface IPacketQueue
{
	void Enqueue(CanFrame frame);
	bool TryDequeue(out CanFrame frame);
}

public static class PacketQueueExtensions
{
	public static void Enqueue(this IPacketQueue queue, IEnumerable<CanFrame> frames)
	{
		foreach (var frame in frames)
		{
			queue.Enqueue(frame);
		}
	}

	public static void Enqueue(this IPacketQueue queue, ReadOnlySpan<CanFrame> frames)
	{
		foreach (var frame in frames)
		{
			queue.Enqueue(frame);
		}
	}
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
		WaveSculptorStatus wsStatus = new WaveSculptorStatus((WaveSculptorStatus.LimitFlags)(rand.Next() & ((1 << 6) - 1)), (WaveSculptorStatus.ErrorFlags)(rand.Next() & ((1 << 9) - 1)), 0, 0, 0);
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

	private void GenerateMpptOutputPower(object? state)
	{
		if (state is not Random rand)
			return;

		uint id = ElmarBase.BaseId + (byte)(rand.Next() << 4) + (byte)BroadcastId.OutputMeasurements;
		OutputMeasurements packet = new OutputMeasurements(id, rand.NextFloat(100, 115), rand.NextFloat(1, 2));

		Queue.Enqueue(packet.ToCanFrame());
	}

	private void GenerateDistributionStatus(object? state)
	{
		if (state is not Random rand)
			return;

		PsrCanId id = new PsrCanId(PsrCanId.MulticastDestination, CanIds.DistributionBase, (byte)DriverInterface.Shared.CanPackets.Distribution.MessageId.Status, CanIds.DeviceType.Distribution, CanIds.MessagePriority.Normal);
		DistributionStatus packet = new DistributionStatus(id.ToInteger(), DistributionStatus.StatusFlags.Mask & (DistributionStatus.StatusFlags)rand.Next());
		Queue.Enqueue(packet.ToCanFrame());
	}

	private void GenerateDistributionBusVoltages(object? state)
	{
		if (state is not Random rand)
			return;

		const float busVoltageScale = 0.003125f;
		const float minVoltage = 11.5f;
		const float maxVoltage = 14.1f;

		short mainVoltage = (short)(rand.NextFloat(minVoltage, maxVoltage) / busVoltageScale);
		short auxVoltage = (short)(rand.NextFloat(minVoltage, maxVoltage) / busVoltageScale);
		PsrCanId id = new PsrCanId(PsrCanId.MulticastDestination, CanIds.DistributionBase, (byte)DriverInterface.Shared.CanPackets.Distribution.MessageId.BusVoltages, CanIds.DeviceType.Distribution, CanIds.MessagePriority.Normal);
		BusVoltages packet = new BusVoltages(id.ToInteger(), mainVoltage, auxVoltage, busVoltageScale);
		Queue.Enqueue(packet.ToCanFrame());
	}

	private void GenerateDistributionCurrents(object? state)
	{
		if (state is not Random rand)
			return;

		const float currentScale = 9.765625E-4f;
		const float minCurrent = 1;
		const float maxCurrent = 15;

		short mainCurrent = (short)(rand.NextFloat(minCurrent, maxCurrent) / currentScale);
		short auxCurrent = (short)(rand.NextFloat(minCurrent, maxCurrent) / currentScale);
		PsrCanId id = new PsrCanId(PsrCanId.MulticastDestination, CanIds.DistributionBase, (byte)DriverInterface.Shared.CanPackets.Distribution.MessageId.Currents, CanIds.DeviceType.Distribution, CanIds.MessagePriority.Normal);
		Currents packet = new Currents(id.ToInteger(), mainCurrent, auxCurrent, currentScale);
		Queue.Enqueue(packet.ToCanFrame());
	}

	private void GenerateDistributionPowers(object? state)
	{
		if (state is not Random rand)
			return;

		const float powerScale = 0.05f;
		const float minPower = 10;
		const float maxPower = 100;

		ushort mainPower = (ushort)(rand.NextFloat(minPower, maxPower) / powerScale);
		ushort auxPower = (ushort)(rand.NextFloat(minPower, maxPower) / powerScale);
		PsrCanId id = new PsrCanId(PsrCanId.MulticastDestination, CanIds.DistributionBase, (byte)DriverInterface.Shared.CanPackets.Distribution.MessageId.Powers, CanIds.DeviceType.Distribution, CanIds.MessagePriority.Normal);
		Powers powers = new Powers(id.ToInteger(), auxPower, mainPower, powerScale);
		Queue.Enqueue(powers.ToCanFrame());
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using Timer wsStatusTimer = new Timer(GenerateWSStatusPacket, new Random(), TimeSpan.Zero, TimeSpan.FromMilliseconds(200));
		using Timer wsBusMeasurement = new Timer(GenerateWSBusMeasurement, new Random(), TimeSpan.FromMilliseconds(2), TimeSpan.FromMilliseconds(200));
		using Timer wsVelocity = new Timer(GenerateWSVelocity, new Random(), TimeSpan.FromMilliseconds(4), TimeSpan.FromMilliseconds(200));

		using Timer steering = new Timer(GenerateSteeringPacket, new Random(), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(100));

		using Timer mpptOutput = new Timer(GenerateMpptOutputPower, new Random(), TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(200));

		using Timer distributionStatus = new Timer(GenerateDistributionStatus, new Random(), TimeSpan.FromMilliseconds(20), TimeSpan.FromMilliseconds(100));
		using Timer distributionBusVoltages = new Timer(GenerateDistributionBusVoltages, new Random(), TimeSpan.FromMilliseconds(30), TimeSpan.FromMilliseconds(100));
		using Timer distributionCurrents = new Timer(GenerateDistributionCurrents, new Random(), TimeSpan.FromMilliseconds(40), TimeSpan.FromMilliseconds(100));
		using Timer distributionPowers = new Timer(GenerateDistributionPowers, new Random(), TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100));

		using Timer trashStandard = new Timer(obj =>
		{
			if (obj is not Random rand)
				return;
			byte[] bytes = new byte[8];
			uint id = (uint)rand.Next(0, 0x7FF);
			rand.NextBytes(bytes);
			Queue.Enqueue(new CanFrame(id, bytes));
		}, new Random(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

		using Timer trashExtended = new Timer(obj =>
		{
			if (obj is not Random rand)
				return;
			byte[] bytes = new byte[8];
			uint id = (uint)rand.Next(0, 0x1FFFFFFF) | 0x80000000;
			rand.NextBytes(bytes);
			Queue.Enqueue(new CanFrame(id, bytes));
		}, new Random(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

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

	public static uint NextUInt32(this Random rand)
	{
		Span<byte> buffer = stackalloc byte[4];
		rand.NextBytes(buffer);
		return BitConverter.ToUInt32(buffer);
	}
}
