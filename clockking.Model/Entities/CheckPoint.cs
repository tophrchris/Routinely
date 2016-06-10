using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace ClockKing.Core
{
	public class CheckPoint
	{
		private List<Occurrence> occurrences;
        private List<ScheduledTargetTime> scheduledTargets;

        private DateTime createdOn;
		public CheckPoint()
		{
			this.occurrences = new List<Occurrence> ();
            this.scheduledTargets = new List<ScheduledTargetTime>();
			this.Enabled = true;
		}
		public string Name { get; set; }
		public string Color { get; set; }
		public string Emoji { get; set; }
		public bool Enabled { get; set; }
        public TimeSpan TargetTime
        { 
            get;
            set;
        }

		[JsonIgnore]
		public IEnumerable<Occurrence> Occurrences { get{return this.occurrences; }}

        public IEnumerable<ScheduledTargetTime> ScheduledTargets{ get { return this.scheduledTargets; } }

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
        public void RemoveOccurrence(Occurrence toRemove)
        {
            this.occurrences.Remove(toRemove);
        }

        public ScheduledTargetTime AddScheduledtarget(TimeSpan? scheduledTargetTime, List<DayOfWeek> days)
        {
            var alt = new ScheduledTargetTime()
                {TargetTime=scheduledTargetTime,
                    ApplicableDays=days.ToArray()};
            
            this.scheduledTargets.Add(alt);
            return alt;
        }

        public bool RemoveScheduledTarget(ScheduledTargetTime toRemove)
        {
            return this.scheduledTargets.Remove(toRemove);
        }    


        protected TimeSpan ScheduledTargetTime
        {
            get
            {
                return   TargetTimeForDay(DateTime.Now.DayOfWeek);
            }
        }

        public bool Active
        {
            get
            {
                return ActiveForDay(DateTime.Today.DayOfWeek);
            }

        }

        public bool ActiveForDay(DayOfWeek day)
        {
            var relevantAlternatives = this.scheduledTargets.Where(t => t.ApplicableDays.Contains(day));
            if (relevantAlternatives.Any(t => !t.TargetTime.HasValue))
                return false;
            
            return true;
        }
            
        public DateTime CreatedOn 
        {
            get{
                //if (this.createdOn != DateTime.MinValue)
                //    return this.createdOn;
                
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
            

		public DateTime MostRecentOccurrenceTimeStamp()
        {
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

        public TimeSpan TargetTimeForDay(DayOfWeek day)
        {
            if (scheduledTargets.Any())
            {
                var f= this.scheduledTargets
                    .FirstOrDefault(t => t.ApplicableDays.Contains(day));

                return f?.TargetTime ?? this.TargetTime;
                
            }
            else
                return TargetTime;
              

        }

		public DateTime TargetTimeToday
		{
			get
            {
                return (DateTime.Today + this.ScheduledTargetTime);
            }
		
		}
            
		public bool CompletedToday 
		{
			get
			{
                if (!this.occurrences.Any())
                    return false;
                
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
				return this.ScheduledTargetTime < DateTime.Now.TimeOfDay;
			}
		}
		public bool TargetTimeUpcoming 
		{
			get 
			{
				return !this.TargetTimePassed;
			}
		}

        protected IEnumerable<TimeSpan> DistinctTimes
        {
            get
            {
                return this.occurrences.Select(o => o.Time).Distinct();
            }
        }

        public Occurrence Earliest
        {
            get
            {
                var em= this.DistinctTimes.OrderBy(o => o.TotalMinutes).FirstOrDefault().TotalMinutes;
                return this.occurrences.OrderByDescending(o => o.TimeStamp).FirstOrDefault(o => o.Time.TotalMinutes == em);
            }
               
        }
        public Occurrence Latest
        {
            get
            {
                var em= this.DistinctTimes.OrderByDescending(o => o.TotalMinutes).FirstOrDefault().TotalMinutes;
                return this.occurrences.OrderByDescending(o => o.TimeStamp).FirstOrDefault(o => o.Time.TotalMinutes == em);
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

