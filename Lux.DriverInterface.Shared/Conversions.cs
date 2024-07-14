using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public static class Conversions
{
    public const float MpsToMph = 2.23693629f;
    public const float MphToMps = 0.44704f;

    public const float MpsToKph = 3.6f;
    public const float KphToMps = 0.277777778f;

    public const float WattToKW = 0.001f;
    public const float KWToWatt = 1000f;
}
