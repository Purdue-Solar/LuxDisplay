namespace Lux.DriverInterface.Shared
{
	using System;
	using System.Threading;

	public class TimerService : IDisposable
	{
		private Timer _timer;
		private int _count = 0;

		public event EventHandler<int> OnTimerElapsed;

		public TimerService()
		{
			_timer = new Timer(TimerCallback, null, 0, 20); // 1-second interval
		}

		private void TimerCallback(object state)
		{
			_count++;
			OnTimerElapsed?.Invoke(this, _count);
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}

}
