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
using Lux.DriverInterface.Shared.CanPackets.Peripherals;
using Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using SocketCANSharp;
using SocketCANSharp.Network;

using WavesculptorStatus = Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast.Status;
using PeripheralsStatus = Lux.DriverInterface.Shared.CanPackets.Peripherals.Status;

namespace Lux.DataRadio
{

	public class CANReceiveService(WaveSculptor wsc, Header header, EMU emu, Telemetry telemetry, CanDecoder decoder) : BackgroundService
	{
		protected WaveSculptor WaveSculptor { get; } = wsc;
		protected Header Header { get; } = header;
		protected Telemetry Telemetry { get; } = telemetry;

		protected CanDecoder Decoder { get; } = decoder;

		private void Init()
		{
			Decoder.AddPacketDecoder((WavesculptorStatus status) =>
			{
				WaveSculptor.LimitFlags = status.Limits;
				WaveSculptor.ErrorFlags = status.Errors;
			});

			Decoder.AddPacketDecoder((BusMeasurement bus) =>
			{
				WaveSculptor.BusCurrent = bus.BusCurrent;
				WaveSculptor.BusVoltage = bus.BusVoltage;
			});

			Decoder.AddPacketDecoder((VelocityMeasurement velocity) =>
			{
				WaveSculptor.VehicleVelocity = velocity.VehicleVelocity;
				WaveSculptor.MotorVelocity = velocity.MotorVelocity;
			});

			Decoder.AddPacketDecoder((PhaseCurrent current) =>
			{
				WaveSculptor.PhaseBCurrent = current.PhaseBCurrent;
				WaveSculptor.PhaseCCurrent = current.PhaseCCurrent;
			});

			Decoder.AddPacketDecoder((VoltageVector voltage) =>
			{
				WaveSculptor.Vd = voltage.Vd;
				WaveSculptor.Vq = voltage.Vq;
			});

			Decoder.AddPacketDecoder((CurrentVector current) =>
			{
				WaveSculptor.CurrentD = current.CurrentD;
				WaveSculptor.CurrentQ = current.CurrentQ;
			});

			Decoder.AddPacketDecoder((BackEmf bemf) =>
			{
				WaveSculptor.BemfD = bemf.BemfD;
				WaveSculptor.BemfQ = bemf.BemfQ;
			});

			Decoder.AddPacketDecoder((Voltage15V voltage) => WaveSculptor.Voltage15 = voltage.Voltage15);

			Decoder.AddPacketDecoder((HeatSinkMotorTemp temp) =>
			{
				WaveSculptor.HeatsinkTemp = temp.HeatSinkTemp;
				WaveSculptor.MotorTemp = temp.MotorTemp;
			});

			Decoder.AddPacketDecoder((DspBoardTemp temp) => WaveSculptor.DspBoardTemp = temp.DspTemp);

			Decoder.AddPacketDecoder((OdometerBusAmpHrs odo) =>
			{
				WaveSculptor.DcBusAmpHrs = odo.DcBusAmpHrs;
				WaveSculptor.Odometer = odo.Odometer;
			});

			Decoder.AddPacketDecoder((SlipSpeed slip) => WaveSculptor.SlipSpeed = slip.SlipSpeedHz);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Init();

			int bytesRead = 0;
			using var rawCanSocket = new RawCanSocket();
			CanNetworkInterface can0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("can0"));

			rawCanSocket.Bind(can0);
			while (!stoppingToken.IsCancellationRequested)
			{
				//Reading from CAN
				CanFrame frame = default;
				bytesRead = await Task.Run(() => rawCanSocket.Read(out frame));
				if (bytesRead != 0)
				{
					uint id = frame.CanId & 0x1FFFFFFF;
					bool isExtended = frame.CanId >> 31 == 1;
					Decoder.HandleCanPacket(id, isExtended, frame.Data);
				}
				else
					await Task.Delay(1);
			}
		}
	}
}
