using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Buffers.Binary;
using System.Device.Spi;
using SocketCANSharp;
using SocketCANSharp.Network;
using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Configuration;
using System.Numerics;
using Encoder = Lux.DriverInterface.Shared.Encoder;
using System.Device.Gpio;
using Lux.DriverInterface.Shared.CanPackets.WaveSculptor.Command;

namespace Lux.DataRadio;

public class PedalService(Encoder amt, SteeringWheel steering, CanSendService canSend, GpioWrapper wrapper, IConfiguration config) : BackgroundService
{
	protected Encoder Amt = amt;
	protected SteeringWheel SteeringWheel { get; } = steering;
	protected CanSendService CanSend { get; } = canSend;
	protected IConfiguration Configuration { get; } = config;
	protected GpioWrapper Controller { get; } = wrapper;
	protected SpiDevice? Spi { get; private set; }

	private ushort _zeroValue = 0;
	private double _lastPercent = 0;
	private double Deadzone { get; } = config.GetValue($"{nameof(PedalService)}:{nameof(Deadzone)}", 2.5);
	private double FullAngle { get; } = config.GetValue($"{nameof(PedalService)}:{nameof(FullAngle)}", 20.0);
	private double MaxSpeed { get; } = config.GetValue($"{nameof(PedalService)}:{nameof(MaxSpeed)}", 60.0);
	private double ReverseMultiplier { get; } = config.GetValue($"{nameof(PedalService)}:{nameof(ReverseMultiplier)}", 1.0);

	private GpioPin ForwardPin { get; } = new GpioPin(
		wrapper,
		config.GetValue($"{nameof(PedalService)}:{nameof(ForwardPin)}:{nameof(GpioPin.PinNumber)}", 6),
		config.GetValue($"{nameof(PedalService)}:{nameof(ForwardPin)}:{nameof(GpioPin.PinMode)}", PinMode.InputPullUp),
		config.GetValue($"{nameof(PedalService)}:{nameof(ForwardPin)}:{nameof(GpioPin.InvertActive)}", true));
	private GpioPin ReversePin { get; } = new GpioPin(
		wrapper,
		config.GetValue($"{nameof(PedalService)}:{nameof(ReversePin)}:{nameof(GpioPin.PinNumber)}", 5),
        config.GetValue($"{nameof(PedalService)}:{nameof(ReversePin)}:{nameof(GpioPin.PinMode)}", PinMode.InputPullUp),
		config.GetValue($"{nameof(PedalService)}:{nameof(ReversePin)}:{nameof(GpioPin.InvertActive)}", true));

	//private GpioPin RegenEnablePin { get; } = new GpioPin(
	//	wrapper,
	//	config.GetValue($"{nameof(PedalService)}:{nameof(RegenEnablePin)}:{nameof(GpioPin.PinNumber)}", 7),
	//	PinMode.InputPullUp,
	//	config.GetValue($"{nameof(PedalService)}:{nameof(RegenEnablePin)}:{nameof(GpioPin.InvertActive)}", true));

	//private GpioPin RegenLedPin { get; } = new GpioPin(
	//	wrapper,
	//	config.GetValue($"{nameof(PedalService)}:{nameof(RegenLedPin)}:{nameof(GpioPin.PinNumber)}", 26),
	//	config.GetValue($"{nameof(PedalService)}:{nameof(RegenLedPin)}:{nameof(GpioPin.PinMode)}", PinMode.Output),
	//	config.GetValue($"{nameof(PedalService)}:{nameof(RegenLedPin)}:{nameof(GpioPin.InvertActive)}", false));

	private const int EncoderBits = 14;
	private const ushort Range = 1 << EncoderBits;
	private const short MinValue = -(1 << (EncoderBits - 1));
	private const short MaxValue = 1 << (EncoderBits - 1);
	private const double StepToAngle = 360.0 / Range;

	private const double HighRpm = 20000;
	private const double LowRpm = -20000;

	private static double PeadalCurve(double x) => Math.Pow(x, 2);

	private void HandlePedal(object? state)
	{
		if (Spi is null)
			return;

		// If the cruise control is active, ignore the pedal position
		// and don't send any commands
		if (SteeringWheel.CruiseActive)
			return;

		Encoder.PedalState pedalState = GetPedalState();
		//pedalState = Encoder.PedalState.Forward; // Force forward for testing
		if (pedalState == Encoder.PedalState.Neutral)   // Neutral ignores the pedal position and doesn't send any commands
			return;

		Amt.State = pedalState;

		Span<byte> writeBuffer = [0x00, 0x00];
		Span<byte> readBuffer = stackalloc byte[2];

		Spi.TransferFullDuplex(writeBuffer, readBuffer);

		// Try to read the value until the checksum is correct
		ushort rawValue = BinaryPrimitives.ReadUInt16BigEndian(readBuffer);
		ushort value;

		const int maxTries = 10;
		int tries = 0;
		while (!ValidateChecksum(rawValue, out value) && tries < maxTries)
		{
			Thread.Sleep(20);
			Spi.TransferFullDuplex(writeBuffer, readBuffer);
			rawValue = BinaryPrimitives.ReadUInt16BigEndian(readBuffer);

			tries++;
		}

		if (tries == maxTries)
			return;

		// Maps the difference correctly to the range of the encoder
		int diff = value - _zeroValue;
		int position;
		if (diff < MinValue)
			position = diff + Range;
		else if (diff >= MaxValue)
			position = diff - Range;
		else
			position = diff;

		double angle = position * StepToAngle;

		double rawPercent = angle / FullAngle;
		double deadzone = Deadzone / FullAngle;

		// Clamp the pedal position to the range [0,1], applying the deadzone
		double percent;
		if (rawPercent < deadzone)
			percent = 0;
		else if (rawPercent > 1)
			percent = 1;
		else
			percent = (rawPercent - deadzone) / (1 - deadzone);

		_lastPercent = percent;

		// Apply the pedal curve
		percent = PeadalCurve(percent);

		Amt.Percentage = (float)percent;
		Amt.Value = value;

		// Reverse pedal
		if (pedalState == Encoder.PedalState.Reverse)
			percent *= -ReverseMultiplier;

		Encoder.ControlMode mode = UpdateControlMode();
		Amt.Mode = mode;

		Drive drive = mode == Encoder.ControlMode.Speed ? GetSpeedControl(percent) : GetTorqueControl(percent);
		CanSend.SendPacket(drive);
	}

	private Drive GetSpeedControl(double percent)
	{
		float rpm = (float)(percent * MaxSpeed);
		float current = 1;
		return new Drive(rpm, current);
	}

	private Drive GetTorqueControl(double percent)
	{
		float rpm = percent >= 0 ? (float)HighRpm : (float)LowRpm;
		float current = (float)percent;
		return new Drive(rpm, current);
	}

	private Encoder.PedalState GetPedalState()
	{
		bool forwardPressed = ForwardPin.Read();
		bool reversePresesd = ReversePin.Read();

		
		return (forwardPressed, reversePresesd) switch
		{
			(false, false) => Encoder.PedalState.Neutral,
			(true, false) => Encoder.PedalState.Forward,
			(false, true) => Encoder.PedalState.Reverse,
			(true, true) => Encoder.PedalState.Neutral
		};
	}

	private Encoder.ControlMode UpdateControlMode()
	{
		return Encoder.ControlMode.Speed;

		//bool regenEnable = RegenEnablePin.Read();
		//RegenLedPin.Write(regenEnable);

		//return regenEnable ? Encoder.ControlMode.Speed : Encoder.ControlMode.Torque;
	}

	private static bool ValidateChecksum(ushort rawValue, out ushort value)
	{
		const uint evenMask = 0b00010101_01010101;
		const uint oddMask = 0b00101010_10101010;

		value = (ushort)(rawValue & ((1u << EncoderBits) - 1u));

		uint k0 = (rawValue & (1u << 14)) >> 14;
		uint k1 = (rawValue & (1u << 15)) >> 15;

		// Calculate even and odd bit parity using popcount instead of xor
		int evenCount = BitOperations.PopCount(rawValue & evenMask);
		int oddCount = BitOperations.PopCount(rawValue & oddMask);

		bool evenParity = (evenCount & 1) != k0;
		bool oddParity = (oddCount & 1) != k1;

		return evenParity & oddParity;
	}

	private void GetZeroPoint()
	{
		if (Spi is null)
			return;

		Span<byte> writeBuffer = [0x00, 0x60];
		Span<byte> readBuffer = stackalloc byte[2];

		Spi.TransferFullDuplex(writeBuffer, readBuffer);

		Thread.Sleep(200);

		writeBuffer[1] = 0x00;
		ushort rawValue = BinaryPrimitives.ReadUInt16BigEndian(readBuffer);
		ushort value;

		const int maxTries = 10;
		int tries = 0;
		while (!ValidateChecksum(rawValue, out value) && tries < maxTries)
		{
			Thread.Sleep(20);
			Spi.TransferFullDuplex(writeBuffer, readBuffer);
			rawValue = BinaryPrimitives.ReadUInt16BigEndian(readBuffer);
			tries++;
		}

		//if (tries == maxTries)
		//	throw new InvalidOperationException("Failed to get zero point");

		_zeroValue = value;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (Environment.OSVersion.Platform != PlatformID.Unix)
			return;

		//Set zero position for encoder
		SpiConnectionSettings settings = new SpiConnectionSettings(1, 0);
		Configuration.Bind($"{nameof(PedalService)}:{nameof(SpiConnectionSettings)}", settings);

		Spi = SpiDevice.Create(settings);

		GetZeroPoint();

		using Timer timer = new Timer(HandlePedal, null, TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(50));

		await Task.Delay(-1, stoppingToken);
	}

	public override void Dispose()
	{
		ForwardPin?.Dispose();
		ReversePin?.Dispose();
		Spi?.Dispose();

		base.Dispose();
		GC.SuppressFinalize(this);
	}
}