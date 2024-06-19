using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lux.DriverInterface.Shared;
public class SteeringWheel
{
	public bool PushToTalkActive { get; set; }
	public bool HeadlightsActive { get; set; }
	public bool RightTurnActive { get; set; }
	public bool HazardsActive { get; set; }
	public bool LeftTurnActive { get; set; }
	public bool CruiseActive { get; set; }
	public bool CruiseUpActive { get; set; }
	public bool CruiseDownActive { get; set; }
	public bool HornActive { get; set; }
	public byte Page { get; set; }
	public float TargetSpeed { get; set; }
}
