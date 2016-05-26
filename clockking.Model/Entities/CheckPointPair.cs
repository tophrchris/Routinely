using System;

namespace ClockKing.Model
{
	public class CheckPointPair
	{
		public CheckPoint firstEvent;
		public CheckPoint secondEvent;

		public CheckPointPair(CheckPoint first, CheckPoint second)
		{
			this.firstEvent = first;
			this.secondEvent = second;
		}
		public TimeSpan InterveningTime 
		{
			get{
				if (this.secondEvent.averageObservedTime < this.firstEvent.averageObservedTime) 
				{
					var untilMidnight = TimeSpan.FromHours (24) - this.firstEvent.averageObservedTime;
					return untilMidnight + this.secondEvent.averageObservedTime;
				}
				return this.secondEvent.averageObservedTime - this.firstEvent.averageObservedTime;
			}
		}
		public override string ToString ()
		{
			return string.Format ("{5}{0} at {1} then wait {2} for {3} at {4}",
				this.firstEvent.Name,
				this.firstEvent.averageObservedTime,
				this.InterveningTime,
				this.secondEvent.Name,
				this.secondEvent.averageObservedTime,
				this.firstEvent.Emoji);
		}
	}
}

