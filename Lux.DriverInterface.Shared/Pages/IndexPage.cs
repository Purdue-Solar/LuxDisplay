using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.Pages;
public class IndexPage(WaveSculptor wsc, Telemetry telem)
{
    protected WaveSculptor WaveSculptor { get; } = wsc;
    protected Telemetry Telemetry { get; } = telem;

    /// <summary>
    /// Vehicle Velocity (m/s)
    /// </summary>
    public float Speed => WaveSculptor.VehicleVelocity;
    /// <summary>
    /// Cabin Temperature (degrees Celsius)
    /// </summary>
    public float CabinTemp => Telemetry.CabinTemperature;

}
