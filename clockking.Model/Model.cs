using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ClockKing.Model
{
	public class CheckPoint
	{
		private List<Occurrence> occurrences;
		public CheckPoint()
		{
			this.occurrences = new List<Occurrence> ();
			this.Enabled = true;
		}
		public string Name { get; set; }
		public string Color { get; set; }
		public string Emoji { get; set; }
		public bool Enabled { get; set; }
		public TimeSpan TargetTime { get; set; }

		public IEnumerable<Occurrence> Occurrences { get{return this.occurrences; }}

		public void AddOccurrence(Occurrence newOccurance)
		{
			this.occurrences.Add (newOccurance);
		}
		public Occurrence CreateOccurrence()
		{
			return this.CreateOccurrence (DateTime.Now.ToLocalTime());	
		}

		public Occurrence CreateOccurrence(DateTime observationTimeStamp)
		{
			return new Occurrence(this,observationTimeStamp);
		}
		public TimeSpan averageObservedTime
		{
			get{
				if (!occurrences.Any ())
					return this.TargetTime;

				var avgminutes = this.occurrences.Average (o => o.Time.TotalMinutes);
				return TimeSpan.FromMinutes (avgminutes);
			}
		}
		public DateTime MostRecentOccurrenceTimeStamp(DateTime ifNone)
		{
				return this.occurrences
				.OrderByDescending (o => o.timeStamp)
				.Select (o => o.timeStamp)
					.DefaultIfEmpty (ifNone)
				.FirstOrDefault ();
		}

		public TimeSpan SinceLastOccurrence
		{
			get{
				var now = DateTime.Now;
				var mostRecent = this.MostRecentOccurrenceTimeStamp(now);
				return now - mostRecent;
			}
		}

		public TimeSpan UntilNextTargetTime
		{
			get
			{
				var now = DateTime.Now;
				var adjustment = this.TargetTime > now.TimeOfDay ? 0 : 1;
				var next = DateTime.Today.AddDays (adjustment) + TargetTime;
				return next - now;

			}
		}


		public override string ToString ()
		{
			return string.Format ("{3}{0}: target={1}, avg={2}", 
				this.Name,
				this.TargetTime,
				this.averageObservedTime,
				this.Emoji);
		}
	}

	public class Occurrence
	{

		private CheckPoint checkPoint;

		public DateTime timeStamp; 

		public Occurrence(CheckPoint assignedCheckPoint,DateTime observedTime)
		{
			this.checkPoint = assignedCheckPoint;
			this.timeStamp = observedTime;
		}

		public string checkpointLabel { get { return this.CheckPoint.Name; } }

		[JsonIgnore]
		public CheckPoint CheckPoint { get { return this.checkPoint; } }

		[JsonIgnore]
		public TimeSpan Time 
		{
			get
			{
				return this.timeStamp.TimeOfDay;
			}
		}
	}

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


