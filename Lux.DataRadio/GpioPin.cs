using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DataRadio;
public record GpioPin : IDisposable
{
    public int PinNumber { get; }
    public PinMode PinMode { get; }
    public bool InvertActive { get; }

    protected GpioWrapper GpioWrapper { get; }
    private System.Device.Gpio.GpioPin? Pin { get; set; }
    bool _disposed = false;

    public GpioPin(GpioWrapper wrapper, int pinNumber, PinMode mode, bool invertActive)
    {
        GpioWrapper = wrapper;
        PinNumber = pinNumber;
        PinMode = mode;
        InvertActive = invertActive;

        if (GpioWrapper.IsOSEnabled)
        {
            Pin = wrapper.GpioController!.OpenPin(pinNumber, mode);

            if (PinMode == PinMode.Output)
                Pin?.Write(InvertActive ? PinValue.High : PinValue.Low);
        }
    }

    public bool Read() => !_disposed ? Pin?.Read() == (InvertActive ? PinValue.Low : PinValue.High) : throw new ObjectDisposedException(nameof(GpioPin));

    public void Write(bool value)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        Pin?.Write(value ^ InvertActive ? PinValue.Low : PinValue.High);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (Pin is not null)
            GpioWrapper.GpioController?.ClosePin(PinNumber);

        Pin = null;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
