using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Lux.DriverInterface.Shared;
using Lux.DriverInterface.Shared.CanPackets.Peripherals;
using Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast;
using Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
using Lux.DriverInterface.Shared.CanPackets.Steering;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using SocketCANSharp;
using SocketCANSharp.Network;

using WavesculptorStatus = Lux.DriverInterface.Shared.CanPackets.Wavescupltor.Broadcast.Status;
using PeripheralsStatus = Lux.DriverInterface.Shared.CanPackets.Peripherals.Status;
using MpptStatus = Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast.Status;
using SteeringStatus = Lux.DriverInterface.Shared.CanPackets.Steering.Status;
using System.Runtime.CompilerServices;

namespace Lux.DataRadio
{
	public class CanReceiveService(WaveSculptor wsc, MpptCollection mppts, SteeringWheel steering, Telemetry telemetry, CanDecoder decoder) : BackgroundService
	{
		protected WaveSculptor WaveSculptor { get; } = wsc;
		protected MpptCollection Mppts { get; } = mppts;
		protected SteeringWheel SteeringWheel { get; } = steering;
		protected Telemetry Telemetry { get; } = telemetry;

		protected CanDecoder Decoder { get; } = decoder;

		private void Init()
		{
			AddWavesculptorDecoders();
			AddMpptDecoders();
			AddSteeringWheelDecoders();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			Init();

			using var rawCanSocket = new RawCanSocket();
			CanNetworkInterface can0 = CanNetworkInterface.GetAllInterfaces(true).First(iface => iface.Name.Equals("can0"));

			rawCanSocket.Bind(can0);
			while (!stoppingToken.IsCancellationRequested)
			{
				//Reading from CAN
				CanFrame frame = default;
				int bytesRead = await Task.Run(() => rawCanSocket.Read(out frame));
				if (bytesRead != 0)
				{
					bool isExtended = frame.CanId >> 31 == 1;
					uint id = isExtended ? frame.CanId & 0x1FFFFFFF : frame.CanId & 0x7FF;

					Decoder.HandleCanPacket(id, isExtended, frame.Data);
				}
				else
					await Task.Delay(1, stoppingToken);
			}
		}

		private void AddSteeringWheelDecoders()
		{
			Decoder.AddPacketDecoder((SteeringStatus status) =>
			{
				SteeringStatus.ButtonFlags buttons = status.Buttons;

				SteeringWheel.PushToTalkActive = (buttons & SteeringStatus.ButtonFlags.PushToTalk) != 0;
				SteeringWheel.HeadlightsActive = (buttons & SteeringStatus.ButtonFlags.Headlight) != 0;
				SteeringWheel.RightTurnActive = (buttons & SteeringStatus.ButtonFlags.RightTurn) != 0;
				SteeringWheel.HazardsActive = (buttons & SteeringStatus.ButtonFlags.Hazards) != 0;
				SteeringWheel.LeftTurnActive = (buttons & SteeringStatus.ButtonFlags.LeftTurn) != 0;
				SteeringWheel.CruiseActive = (buttons & SteeringStatus.ButtonFlags.Cruise) != 0;
				SteeringWheel.CruiseUpActive = (buttons & SteeringStatus.ButtonFlags.CruiseUp) != 0;
				SteeringWheel.CruiseDownActive = (buttons & SteeringStatus.ButtonFlags.CruiseDown) != 0;
				SteeringWheel.HornActive = (buttons & SteeringStatus.ButtonFlags.Horn) != 0;

				SteeringWheel.Page = status.Page;
			});
		}

		private void AddMpptDecoders()
		{
			Decoder.AddPacketDecoder((InputMeasurements input) =>
			{
				Mppts[input.DeviceId].InputVoltage = input.InputVoltage;
				Mppts[input.DeviceId].InputCurrent = input.InputCurrent;
			});

			Decoder.AddPacketDecoder((OutputMeasurements output) =>
			{
				Mppts[output.DeviceId].OutputVoltage = output.OutputVoltage;
				Mppts[output.DeviceId].OutputCurrent = output.OutputCurrent;
			});

			Decoder.AddPacketDecoder((Temperature temp) =>
			{
				Mppts[temp.DeviceId].MosfetTemperature = temp.MosfetTemperature;
				Mppts[temp.DeviceId].ControllerTemperature = temp.ControllerTemperature;
			});

			Decoder.AddPacketDecoder((AuxiliaryPowerSupply aux) =>
			{
				Mppts[aux.DeviceId].Voltage12V = aux.Voltage12V;
				Mppts[aux.DeviceId].Voltage3V = aux.Voltage3V;
			});

			Decoder.AddPacketDecoder((Limits limits) =>
			{
				Mppts[limits.DeviceId].MaxOutputVoltage = limits.MaxOutputVoltage;
				Mppts[limits.DeviceId].MaxInputCurrent = limits.MaxInputCurrent;
			});

			Decoder.AddPacketDecoder((MpptStatus status) =>
			{
				Mppts[status.DeviceId].RxErrorCount = status.RxErrorCount;
				Mppts[status.DeviceId].TxErrorCount = status.TxErrorCount;
				Mppts[status.DeviceId].TxOverflowCount = status.TxOverflowCount;
				Mppts[status.DeviceId].ErrorFlags = status.Errors;
				Mppts[status.DeviceId].LimitFlags = status.Limits;
				Mppts[status.DeviceId].Mode = status.Mode;
				Mppts[status.DeviceId].TestCounter = status.TestCounter;
			});

			Decoder.AddPacketDecoder((PowerConnector connector) =>
			{
				Mppts[connector.DeviceId].PowerConnectorVoltage = connector.OutputVoltage;
				Mppts[connector.DeviceId].PowerConnectorTemp = connector.ConnectorTemperature;
			});
		}

		private void AddWavesculptorDecoders()
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

			Decoder.AddPacketDecoder((Voltage3V3_1V9 voltage) =>
			{
				WaveSculptor.Voltage1V9 = voltage.Voltage1V9;
				WaveSculptor.Voltage3V3 = voltage.Voltage3V3;
			});

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
	}
}
