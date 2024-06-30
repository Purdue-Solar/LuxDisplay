using Lux.DriverInterface.Shared;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Encoder = Lux.DriverInterface.Shared.Encoder;

namespace Lux.DriverInterface.Client;

#nullable enable
public class BackgroundDataService(HttpClient http, WaveSculptor ws, SteeringWheel steering, MpptCollection mppts, Encoder encoder, ILogger<BackgroundDataService> logger) : IDisposable
{
	protected HttpClient Http { get; set; } = http;
	protected WaveSculptor WaveSculptor { get; set; } = ws;
	protected SteeringWheel SteeringWheel { get; set; } = steering;
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
				SteeringWheel.HeadlightsActive = response.HeadlightsActive;
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

	private const double MpptCollectionPeriod = 1000;
	private async Task RetrieveMpptCollectionDataAsync(CancellationToken token)
	{
		var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(MpptCollectionPeriod));
		_timers.Add(timer);

		while (!token.IsCancellationRequested)
		{
			try
			{
				MpptCollection? response = await Http.GetFromJsonAsync<MpptCollection>("api/Mppts", token);
				if (response is null)
					return;

				for (int i = 0; i < Math.Min(response.Mppts.Length, MpptCollection.Mppts.Length); i++)
				{
					Mppt mpptResponse = response.Mppts[i];
					Mppt mppt = MpptCollection.Mppts[i];

					mppt.InputVoltage = mpptResponse.InputVoltage;
					mppt.InputCurrent = mpptResponse.InputCurrent;
					mppt.OutputVoltage = mpptResponse.OutputVoltage;
					mppt.OutputCurrent = mpptResponse.OutputCurrent;
					mppt.MosfetTemperature = mpptResponse.MosfetTemperature;
					mppt.ControllerTemperature = mpptResponse.ControllerTemperature;
					mppt.Voltage12V = mpptResponse.Voltage12V;
					mppt.Voltage3V = mpptResponse.Voltage3V;
					mppt.MaxOutputVoltage = mpptResponse.MaxOutputVoltage;
					mppt.MaxInputCurrent = mpptResponse.MaxInputCurrent;
					mppt.RxErrorCount = mpptResponse.RxErrorCount;
					mppt.TxErrorCount = mpptResponse.TxErrorCount;
					mppt.TxOverflowCount = mpptResponse.TxOverflowCount;
					mppt.ErrorFlags = mpptResponse.ErrorFlags;
					mppt.LimitFlags = mpptResponse.LimitFlags;
					mppt.Mode = mpptResponse.Mode;
					mppt.TestCounter = mpptResponse.TestCounter;
					mppt.PowerConnectorVoltage = mpptResponse.PowerConnectorVoltage;
					mppt.PowerConnectorTemp = mpptResponse.PowerConnectorTemp;
				}

				OnChange?.Invoke();

			}
			catch (Exception ex)
			{
				Logger.LogError(ex, "Error retrieving MPPT data");
			}
			
			await timer.WaitForNextTickAsync(token);
		}

		_timers.Remove(timer);
		timer.Dispose();
	}

	private const double EncoderPeriod = 500;
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

	public virtual void Dispose()
	{
		foreach (var timer in _timers)
			timer.Dispose();
		_timers.Clear();

		GC.SuppressFinalize(this);
	}
}
