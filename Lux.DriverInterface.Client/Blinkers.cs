using BlazorBootstrap;
using Lux.DriverInterface.Shared;

namespace Lux.DriverInterface.Client;

public class Blinkers : IDisposable
{
    public static readonly IconColor OnColor = IconColor.Warning;
    public static readonly IconColor OffColor = IconColor.Muted;

    protected SteeringWheel SteeringWheel { get; }

    private const double _blinkRate = 1 / 3.0f;
    private bool _blinker = false;
    private readonly Timer _timer;

    private bool _leftBlinker = false;
    private bool _rightBlinker = false;

    public IconColor LeftColor => _leftBlinker ? OnColor : OffColor;
    public IconColor RightColor => _rightBlinker ? OnColor : OffColor;

    public Blinkers(SteeringWheel steering)
    {
        SteeringWheel = steering;

        _timer = new Timer(BlinkTimer, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(_blinkRate));
    }

    private void BlinkTimer(object state)
    {
        _blinker = !_blinker;

        if (SteeringWheel.HazardsActive)
        {
            _leftBlinker = _blinker;
            _rightBlinker = _blinker;
        }
        else
        {
            _leftBlinker = false;
            _rightBlinker = false;

            if (SteeringWheel.LeftTurnActive)
            {
                _leftBlinker = _blinker;
            }

            if (SteeringWheel.RightTurnActive)
            {
                _rightBlinker = _blinker;
            }
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
