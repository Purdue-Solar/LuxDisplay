using System;
using System.Threading;

namespace Lux.DriverInterface.Shared
{

	public class TimerService : IDisposable
	{
		private readonly Timer _timer;
		private int _count = 0;

		public event EventHandler<int> OnTimerElapsed;

		public TimerService()
		{
			OnTimerElapsed += (_, _) => { };
			_timer = new Timer(TimerCallback, null, 0, 20); // 1-second interval
		}

		private void TimerCallback(object? _)
		{
			_count++;
			OnTimerElapsed?.Invoke(this, _count);
		}

		public virtual void Dispose()
		{
			_timer?.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
