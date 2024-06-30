using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lux.DriverInterface.Shared.CanPackets.Elmar.Broadcast.Status;

namespace Lux.DriverInterface.Shared;
public class MpptCollection
{
	public Mppt[] Mppts { get; } = new Mppt[CanPackets.Elmar.ElmarBase.MaxMpptCount];
	public int Count => Mppts.Length;

	public MpptCollection()
	{
		for (byte i = 0; i < Mppts.Length; i++)
		{
			Mppts[i] = new Mppt(i);
		}
	}

	public Mppt this[byte deviceId]
	{
		get
		{
			if (deviceId >= Mppts.Length)
				throw new ArgumentOutOfRangeException(nameof(deviceId));

			return Mppts[deviceId];
		}
	}

	public float TotalInputPower => Mppts.Sum(mppt => mppt.InputVoltage * mppt.InputCurrent);
	public float TotalOutputPower => Mppts.Sum(mppt => mppt.OutputVoltage * mppt.OutputCurrent);

	public float AverageEffiecency => TotalInputPower == 0 ? 0 : TotalOutputPower / TotalInputPower;

	public ErrorFlags AggregateFlags => Mppts.Aggregate(ErrorFlags.None, (v, mppt) => v | mppt.ErrorFlags);

	public LimitFlags AggregateLimits => Mppts.Aggregate(LimitFlags.None, (v, mppt) => v | mppt.LimitFlags);
}
