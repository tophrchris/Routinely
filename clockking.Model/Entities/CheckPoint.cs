using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace ClockKing.Core
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

		[JsonIgnore]
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

		public DateTime MostRecentOccurrenceTimeStamp(){
			return MostRecentOccurrenceTimeStamp (DateTime.Now);
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

		public DateTime TargetTimeToday
		{
			get{
				return (DateTime.Today + this.TargetTime);
			}
		}

		public bool CompletedToday 
		{
			get
			{
				return this.occurrences.Any (o => o.timeStamp.Date == DateTime.Today);
			}
		}

		public bool TargetTimePassed 
		{
			get 
			{
				return this.TargetTime < DateTime.Now.TimeOfDay;
			}
		}
		public bool TargetTimeUpcoming 
		{
			get 
			{
				return !this.TargetTimePassed;
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
}

