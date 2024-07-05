using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DataRadio;

public class GpioWrapper
{
	// TODO: correct privileges for GPIO on Raspberry Pi
	public static bool IsOSEnabled => Environment.OSVersion.Platform == PlatformID.Unix;
    public GpioController? GpioController { get; }

    public GpioWrapper()
    {
        if (IsOSEnabled)
            GpioController = new GpioController();
    }

    public GpioWrapper(GpioController controller)
    {
        GpioController = controller;
    }
}
