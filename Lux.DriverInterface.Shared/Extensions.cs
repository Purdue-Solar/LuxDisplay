using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public static class Extensions
{
	public static DateTime RoundDown(this DateTime dateTime, TimeSpan interval) => new DateTime(dateTime.Ticks / interval.Ticks * interval.Ticks, dateTime.Kind);
}
