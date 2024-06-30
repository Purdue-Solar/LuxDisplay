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
using Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast;
using Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast;
using Lux.DriverInterface.Shared.CanPackets.Steering;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;
using SocketCANSharp;
using SocketCANSharp.Network;

using WaveSculptorStatus = Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Broadcast.Status;
using PeripheralsStatus = Lux.DriverInterface.Shared.CanPackets.Peripherals.Status;
using MpptStatus = Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast.Status;
using SteeringStatus = Lux.DriverInterface.Shared.CanPackets.Steering.Status;

namespace Lux.DataRadio
{
	public class CanReceiveService(ICanServiceBase serviceBase, WaveSculptor wsc, MpptCollection mppts, PeripheralCollection peripherals, SteeringWheel steering, Telemetry telemetry, CanDecoder decoder) : BackgroundService
	{
		protected ICanServiceBase ServiceBase { get; } = serviceBase;
		protected WaveSculptor WaveSculptor { get; } = wsc;
		protected MpptCollection Mppts { get; } = mppts;
		protected PeripheralCollection Peripherals { get; } = peripherals;
		protected SteeringWheel SteeringWheel { get; } = steering;
		protected Telemetry Telemetry { get; } = telemetry;

		protected CanDecoder Decoder { get; } = decoder;

		private void Init()
		{
			AddWaveSculptorDecoders();
			AddMpptDecoders();
			AddPeripheralDecoders();
			AddSteeringWheelDecoders();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			ServiceBase.Init();
			Init();

			while (!stoppingToken.IsCancellationRequested)
			{
				//Reading from CAN
				CanFrame frame = default;
				int bytesRead = await Task.Run(() => ServiceBase.Read(out frame), stoppingToken);
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

				SteeringWheel.TargetSpeed = status.TargetSpeed;
			});
		}

		private void AddPeripheralDecoders()
		{
			Decoder.AddPacketDecoder((PeripheralsStatus status) =>
			{
				byte id = (byte)(status.CanId.Source - CanIds.PeripheralsBase);

				if (id >= Peripherals.Count)
					return;

				Peripherals[id].Outputs = status.Outputs;
			});
		}

		private void AddMpptDecoders()
		{
			Decoder.AddPacketDecoder((InputMeasurements input) =>
			{
				if (input.DeviceId >= Mppts.Count)
					return;

				Mppts[input.DeviceId].InputVoltage = input.InputVoltage;
				Mppts[input.DeviceId].InputCurrent = input.InputCurrent;
			});

			Decoder.AddPacketDecoder((OutputMeasurements output) =>
			{
				if (output.DeviceId >= Mppts.Count)
					return;

				Mppts[output.DeviceId].OutputVoltage = output.OutputVoltage;
				Mppts[output.DeviceId].OutputCurrent = output.OutputCurrent;
			});

			Decoder.AddPacketDecoder((Temperature temp) =>
			{
				if (temp.DeviceId >= Mppts.Count)
					return;

				Mppts[temp.DeviceId].MosfetTemperature = temp.MosfetTemperature;
				Mppts[temp.DeviceId].ControllerTemperature = temp.ControllerTemperature;
			});

			Decoder.AddPacketDecoder((AuxiliaryPowerSupply aux) =>
			{
				if (aux.DeviceId >= Mppts.Count)
					return;

				Mppts[aux.DeviceId].Voltage12V = aux.Voltage12V;
				Mppts[aux.DeviceId].Voltage3V = aux.Voltage3V;
			});

			Decoder.AddPacketDecoder((Limits limits) =>
			{
				if (limits.DeviceId >= Mppts.Count)
					return;

				Mppts[limits.DeviceId].MaxOutputVoltage = limits.MaxOutputVoltage;
				Mppts[limits.DeviceId].MaxInputCurrent = limits.MaxInputCurrent;
			});

			Decoder.AddPacketDecoder((MpptStatus status) =>
			{
				if (status.DeviceId >= Mppts.Count)
					return;

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
				if (connector.DeviceId >= Mppts.Count)
					return;

				Mppts[connector.DeviceId].PowerConnectorVoltage = connector.OutputVoltage;
				Mppts[connector.DeviceId].PowerConnectorTemp = connector.ConnectorTemperature;
			});
		}

		private void AddWaveSculptorDecoders()
		{
			Decoder.AddPacketDecoder((WaveSculptorStatus status) =>
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

		public override void Dispose()
		{
			if (ServiceBase is IDisposable disposable)
				disposable.Dispose();
			base.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
