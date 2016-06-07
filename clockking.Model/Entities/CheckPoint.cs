using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace ClockKing.Core
{
	public class CheckPoint
	{
		private List<Occurrence> occurrences;
        private List<ScheduledTargetTime> targetTimeAlternatives;
        private TimeSpan targetTime;

        private DateTime createdOn;
		public CheckPoint()
		{
			this.occurrences = new List<Occurrence> ();
            this.targetTimeAlternatives = new List<ScheduledTargetTime>();
			this.Enabled = true;
		}
		public string Name { get; set; }
		public string Color { get; set; }
		public string Emoji { get; set; }
		public bool Enabled { get; set; }
        public TimeSpan TargetTime 
        { 
            get
            {
                return this.targetTime;
            }
            set
            {
                this.targetTime = value;    
            }
        }

		[JsonIgnore]
		public IEnumerable<Occurrence> Occurrences { get{return this.occurrences; }}

        public IEnumerable<ScheduledTargetTime> TargetTimeAlternatives{ get { return this.targetTimeAlternatives; } }

		public void AddOccurrence(Occurrence newOccurance)
		{
            this.occurrences.Add(newOccurance);
		}
		public Occurrence CreateOccurrence()
		{
			return this.CreateOccurrence (DateTime.Now.ToLocalTime());	
		}

		public Occurrence CreateOccurrence(DateTime observationTimeStamp)
		{
			return new Occurrence(this,observationTimeStamp);
        }

        public ScheduledTargetTime AddAlternativeTarget(TimeSpan? alternativeTime, List<DayOfWeek> days)
        {
            var alt = new ScheduledTargetTime()
                {TargetTime=alternativeTime,
                    ApplicableDays=days.ToArray()};
            
            this.targetTimeAlternatives.Add(alt);
            return alt;
        }

        public bool RemoveAlternativeTarget(ScheduledTargetTime toRemove)
        {
            return this.targetTimeAlternatives.Remove(toRemove);
        }    


        protected TimeSpan? AlternativeTargetTime
        {
            get
            {
                var today = DateTime.Now.DayOfWeek;
                var relevantAlternative = this.targetTimeAlternatives
                    .FirstOrDefault(t => t.ApplicableDays.Contains(today));
                
                return relevantAlternative?.TargetTime ?? this.targetTime;
            }
        }

        public bool Active
        {
            get
            {
                var today = DateTime.Now.DayOfWeek;
                var relevantAlternatives = this.targetTimeAlternatives.Where(t => t.ApplicableDays.Contains(today));
                if (relevantAlternatives.Any(t => !t.TargetTime.HasValue))
                    return false;
                
                return true;    
            }

        }

        public DateTime CreatedOn 
        {
            get{
                if (this.createdOn != DateTime.MinValue)
                    return this.createdOn;
                
                if (this.occurrences.Any())
                    return this.occurrences.OrderBy(o => o.TimeStamp).First().TimeStamp;
                else
                    return DateTime.Now;
            }
            set{
                this.createdOn = value;
            }
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
				.OrderByDescending (o => o.TimeStamp)
				.Select (o => o.TimeStamp)
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
				return (DateTime.Today + this.AlternativeTargetTime.Value);
			}
		}

		public bool CompletedToday 
		{
			get
			{
                var mostRecent = this.occurrences
                    .OrderByDescending(o => o.TimeStamp) 
                    .FirstOrDefault();

                return mostRecent.Date == DateTime.Today;
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
    public class ScheduledTargetTime
    {
        public TimeSpan? TargetTime{ get; set; }
        public DayOfWeek[] ApplicableDays{ get; set; }
    }
}

