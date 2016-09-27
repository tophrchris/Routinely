using System;
using System.Diagnostics;
using UIKit;

namespace ClockKing
{
	public class TrackingBenchmark : IDisposable
	{
		private AppDelegate app { get { return UIApplication.SharedApplication.Delegate as AppDelegate; } }
		public string Category { get; set; }
		public string Name { get; set; }
		public Stopwatch Timer { get; set; } = Stopwatch.StartNew();

		public void Track(bool reset = true)
		{
			app.Track(Category, Name, Timer);
			app.Track("Timer",Category, Name);
			if (reset)
				Timer.Reset();
		}
		public void Dispose()
		{
			Track();
			Timer.Stop();
		}
	}
}
