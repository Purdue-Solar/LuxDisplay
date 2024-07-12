using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Encoder = Lux.DriverInterface.Shared.Encoder;

namespace Lux.DriverInterface.Client;

#nullable enable
public class BackgroundDataService(HttpClient http, Telemetry telemetry, WaveSculptor ws, SteeringWheel steering, Distribution distribution, MpptCollection mppts, Encoder encoder, ILogger<BackgroundDataService> logger) : IDisposable
{
	protected HttpClient Http { get; set; } = http;
	protected Telemetry Telemetry { get; set; } = telemetry;
	protected WaveSculptor WaveSculptor { get; set; } = ws;
	protected SteeringWheel SteeringWheel { get; set; } = steering;
	protected Distribution Distribution { get; set; } = distribution;
	protected MpptCollection MpptCollection { get; set; } = mppts;
	protected Encoder Encoder { get; set; } = encoder;
	protected ILogger Logger { get; set; } = logger;

	private readonly HashSet<PeriodicTimer> _timers = [];

	public event Action? OnChange;

	public async Task StartAsync(CancellationToken cancellation = default)
	{
		_ = Task.Run(() => RetrieveWaveSculptorDataAsync(cancellation), cancellation);
		await Task.Delay(50, cancellation);
		_ = Task.Run(() => RetrieveSteeringWheelDataAsync(cancellation), cancellation);
		await Task.Delay(50, cancellation);
		_ = Task.Run(() => RetrieveMpptCollectionDataAsync(cancellation), cancellation);
		await Task.Delay(50, cancellation);
		_ = Task.Run(() => RetrieveEncoderDataAsync(cancellation), cancellation);
		await Task.Delay(50, cancellation);
		_ = Task.Run(() => RetrieveDistributionDataAsync(cancellation), cancellation);
		await Task.Delay(50, cancellation);
		_ = Task.Run(() => RetrieveTelemetryDataAsync(cancellation), cancellation);
	}

	const double WaveSculptorPeriod = 250;
	private async Task RetrieveWaveSculptorDataAsync(CancellationToken token)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(WaveSculptorPeriod));
		_timers.Add(timer);

		while (!token.IsCancellationRequested)
		{
			try
			{
				WaveSculptor? response = await Http.GetFromJsonAsync<WaveSculptor>("api/WaveSculptor", token);
				if (response is null)
					return;

				WaveSculptor.ErrorFlags = response.ErrorFlags;
				WaveSculptor.LimitFlags = response.LimitFlags;
				WaveSculptor.BusCurrent = response.BusCurrent;
				WaveSculptor.BusVoltage = response.BusVoltage;
				WaveSculptor.VehicleVelocity = response.VehicleVelocity;
				WaveSculptor.MotorVelocity = response.MotorVelocity;
				WaveSculptor.PhaseCCurrent = response.PhaseCCurrent;
				WaveSculptor.PhaseBCurrent = response.PhaseBCurrent;
				WaveSculptor.Vd = response.Vd;
				WaveSculptor.Vq = response.Vq;
				WaveSculptor.CurrentD = response.CurrentD;
				WaveSculptor.CurrentQ = response.CurrentQ;
				WaveSculptor.BemfD = response.BemfD;
				WaveSculptor.BemfQ = response.BemfQ;
				WaveSculptor.Voltage15 = response.Voltage15;
				WaveSculptor.Voltage1V9 = response.Voltage1V9;
				WaveSculptor.Voltage3V3 = response.Voltage3V3;
				WaveSculptor.HeatsinkTemp = response.HeatsinkTemp;
				WaveSculptor.MotorTemp = response.MotorTemp;
				WaveSculptor.DspBoardTemp = response.DspBoardTemp;
				WaveSculptor.DcBusAmpHrs = response.DcBusAmpHrs;
				WaveSculptor.Odometer = response.Odometer;
				WaveSculptor.SlipSpeed = response.SlipSpeed;

				OnChange?.Invoke();

			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error retrieving WaveSculptor data");
			}

			await timer.WaitForNextTickAsync(token);
		}

		_timers.Remove(timer);
		timer.Dispose();
	}

	private const double SteeringWheelPeriod = 250;
	private async Task RetrieveSteeringWheelDataAsync(CancellationToken token)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(SteeringWheelPeriod));
		_timers.Add(timer);

		while (!token.IsCancellationRequested)
		{
			try
			{
				SteeringWheel? response = await Http.GetFromJsonAsync<SteeringWheel>("api/SteeringWheel", token);
				if (response is null)
					return;

				SteeringWheel.PushToTalkActive = response.PushToTalkActive;
				SteeringWheel.ControlMode = response.ControlMode;
				SteeringWheel.RightTurnActive = response.RightTurnActive;
				SteeringWheel.HazardsActive = response.HazardsActive;
				SteeringWheel.LeftTurnActive = response.LeftTurnActive;
				SteeringWheel.CruiseActive = response.CruiseActive;
				SteeringWheel.CruiseUpActive = response.CruiseUpActive;
				SteeringWheel.CruiseDownActive = response.CruiseDownActive;
				SteeringWheel.HornActive = response.HornActive;
				SteeringWheel.Page = response.Page;
				SteeringWheel.TargetSpeed = response.TargetSpeed;

				OnChange?.Invoke();

			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error retrieving Steering Wheel data");
			}

			await timer.WaitForNextTickAsync(token);
		}

		_timers.Remove(timer);
		timer.Dispose();
	}

	private const double MpptCollectionPeriod = 1000.0 / 3;
	private int mpptId = 0;
	private async Task RetrieveMpptCollectionDataAsync(CancellationToken token)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(MpptCollectionPeriod));
		_timers.Add(timer);

		while (!token.IsCancellationRequested)
		{
			try
			{
				Mppt? response = await Http.GetFromJsonAsync<Mppt>($"api/Mppts?deviceId={mpptId}", token);
				if (response is null)
					return;

				MpptCollection.Mppts[mpptId].InputVoltage = response.InputVoltage;
				MpptCollection.Mppts[mpptId].InputCurrent = response.InputCurrent;
				MpptCollection.Mppts[mpptId].OutputVoltage = response.OutputVoltage;
				MpptCollection.Mppts[mpptId].OutputCurrent = response.OutputCurrent;
				MpptCollection.Mppts[mpptId].MosfetTemperature = response.MosfetTemperature;
				MpptCollection.Mppts[mpptId].ControllerTemperature = response.ControllerTemperature;
				MpptCollection.Mppts[mpptId].Voltage12V = response.Voltage12V;
				MpptCollection.Mppts[mpptId].Voltage3V = response.Voltage3V;
				MpptCollection.Mppts[mpptId].MaxOutputVoltage = response.MaxOutputVoltage;
				MpptCollection.Mppts[mpptId].MaxInputCurrent = response.MaxInputCurrent;
				MpptCollection.Mppts[mpptId].RxErrorCount = response.RxErrorCount;
				MpptCollection.Mppts[mpptId].TxErrorCount = response.TxErrorCount;
				MpptCollection.Mppts[mpptId].TxOverflowCount = response.TxOverflowCount;
				MpptCollection.Mppts[mpptId].ErrorFlags = response.ErrorFlags;
				MpptCollection.Mppts[mpptId].LimitFlags = response.LimitFlags;
				MpptCollection.Mppts[mpptId].Mode = response.Mode;
				MpptCollection.Mppts[mpptId].TestCounter = response.TestCounter;
				MpptCollection.Mppts[mpptId].PowerConnectorVoltage = response.PowerConnectorVoltage;
				MpptCollection.Mppts[mpptId].PowerConnectorTemp = response.PowerConnectorTemp;

				Console.WriteLine(response);

				OnChange?.Invoke();
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error retrieving MPPT data");
			}

			await timer.WaitForNextTickAsync(token);
			if (++mpptId == MpptCollection.Count)
				mpptId = 0;
		}

		_timers.Remove(timer);
		timer.Dispose();
	}

	private const double EncoderPeriod = 250;
	private async Task RetrieveEncoderDataAsync(CancellationToken token)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(EncoderPeriod));
		_timers.Add(timer);

		while (!token.IsCancellationRequested)
		{
			try
			{
				Encoder? response = await Http.GetFromJsonAsync<Encoder>("api/Encoder", token);
				if (response is null)
					return;

				Encoder.Percentage = response.Percentage;
				Encoder.Value = response.Value;
				Encoder.State = response.State;
				Encoder.Mode = response.Mode;

				OnChange?.Invoke();
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error retrieving Encoder data");
			}

			await timer.WaitForNextTickAsync(token);
		}

		_timers.Remove(timer);
		timer.Dispose();
	}

	private const double DistributionPeriod = 500;
	private async Task RetrieveDistributionDataAsync(CancellationToken token)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(DistributionPeriod));
		_timers.Add(timer);

		while (!token.IsCancellationRequested)
		{
			try
			{
				Distribution? response = await Http.GetFromJsonAsync<Distribution>("api/Distribution", token);
				if (response is null)
					return;

				Distribution.Flags = response.Flags;
				Distribution.RawMainVoltage = response.RawMainVoltage;
				Distribution.RawAuxVoltage = response.RawAuxVoltage;
				Distribution.VoltageScaleFactor = response.VoltageScaleFactor;
				Distribution.RawMainCurrent = response.RawMainCurrent;
				Distribution.RawAuxCurrent = response.RawAuxCurrent;
				Distribution.CurrentScaleFactor = response.CurrentScaleFactor;
				Distribution.RawMainTemperature = response.RawMainTemperature;
				Distribution.RawAuxTemperature = response.RawAuxTemperature;
				Distribution.TemperatureScaleFactor = response.TemperatureScaleFactor;
				Distribution.RawMainPower = response.RawMainPower;
				Distribution.RawAuxPower = response.RawAuxPower;
				Distribution.PowerScaleFactor = response.PowerScaleFactor;
				Distribution.RawMainEnergy = response.RawMainEnergy;
				Distribution.RawAuxEnergy = response.RawAuxEnergy;
				Distribution.EnergyScaleFactor = response.EnergyScaleFactor;

				OnChange?.Invoke();
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error retrieving Distribution data");
			}

			await timer.WaitForNextTickAsync(token);
		}

		_timers.Remove(timer);
		timer.Dispose();
	}

	private const double TelemetryPeriod = 500;
	private async Task RetrieveTelemetryDataAsync(CancellationToken token)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TelemetryPeriod));
		_timers.Add(timer);

		while (!token.IsCancellationRequested)
		{
			try
			{
				Telemetry? response = await Http.GetFromJsonAsync<Telemetry>("api/Telemetry", token);
				if (response is null)
					return;

				Telemetry.BrakesEngaged = response.BrakesEngaged;
				Telemetry.TemperatureWarning = response.TemperatureWarning;
				Telemetry.TemperatureCritical = response.TemperatureCritical;
				Telemetry.BrakePressure1 = response.BrakePressure1;
				Telemetry.BrakePressure2 = response.BrakePressure2;
				Telemetry.CabinTemperature = response.CabinTemperature;
				Telemetry.CabinHumiditiy = response.CabinHumiditiy;
				Telemetry.RealBrakePressure1 = response.RealBrakePressure1;
				Telemetry.RealBrakePressure2 = response.RealBrakePressure2;

				OnChange?.Invoke();
			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error retrieving Telemetry data");
			}

			await timer.WaitForNextTickAsync(token);
		}

		_timers.Remove(timer);
		timer.Dispose();
	}

	public virtual void Dispose()
	{
		foreach (var timer in _timers)
			timer.Dispose();
		_timers.Clear();

		GC.SuppressFinalize(this);
	}
}
