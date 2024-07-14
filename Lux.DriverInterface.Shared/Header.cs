namespace Lux.DriverInterface.Shared
{
	public class Header
	{
		public int Speed { get; set; }
		public int RPM { get; set; }
		public bool AlertFlagWarning { get; set; } = false;
		public bool AlertFlagError { get; set; } = false;
		public bool BlinkerToggleLeft { get; set; } = false;
		public bool BlinkerToggleRight { get; set; } = false;
		public bool Hazard { get; set; } = false;
		public bool Overheat { get; set; } = false;
		public bool HeartbeatFailure { get; set; } = false;
		public bool Headlights { get; set; } = false;
		public void ToggleAlertFlagWarning()
		{
			AlertFlagWarning = !AlertFlagWarning;
		}

		public void ToggleAlertFlagError()
		{
			AlertFlagError = !AlertFlagError;
		}
	}
}
