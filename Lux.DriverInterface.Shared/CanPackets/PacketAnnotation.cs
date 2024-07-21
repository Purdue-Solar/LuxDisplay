using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared.CanPackets;

[AttributeUsage(AttributeTargets.Property)]
public class FieldScaleAttribute(double scaleFactor) : Attribute
{
	public double ScaleFactor { get; set; } = scaleFactor;
}

[AttributeUsage(AttributeTargets.Property)]
public class FieldLabelAttribute(string label) : Attribute
{
	public string Label { get; set; } = label;
}
