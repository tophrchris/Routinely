using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace ClockKing.Core
{
	public class CheckPoint
	{
        private List<Occurrence> allOccurrences;
        private IEnumerable<Occurrence> occurrences
        {
            get { return allOccurrences.Where (o => !o.IsSkipped); }
        }
        private List<ScheduledTargetTime> scheduledTargets;

        private DateTime createdOn;

		public CheckPoint()
		{
			this.allOccurrences = new List<Occurrence> ();
            this.scheduledTargets = new List<ScheduledTargetTime>();
			this.Enabled = true;
		}

        public Guid UniqueIdentifier { get; set; }
		public string Name { get; set; }
		public string Color { get; set; }
		public string Emoji { get; set; }
		public bool Enabled { get; set; }
        public string Category { get; set; }
        public TimeSpan TargetTime
        { 
            get;
            set;
        }

        public DateTime CreatedOn {
            get {

                return this.createdOn;
            }
            set {
                this.createdOn = value;
            }
        }

        [JsonIgnore]
        public IEnumerable<Occurrence> AllOccurrences {get {return this.allOccurrences;}}

		[JsonIgnore]
		public IEnumerable<Occurrence> Occurrences { get{return this.occurrences; }}

        public IEnumerable<ScheduledTargetTime> ScheduledTargets{ get { return this.scheduledTargets; } }


		public void AddOccurrence(Occurrence newOccurance)
		{
            this.allOccurrences.Add(newOccurance);
		}
		public Occurrence CreateOccurrence()
		{
			return this.CreateOccurrence (DateTime.Now.ToLocalTime());	
		}

		public Occurrence CreateOccurrence(DateTime observationTimeStamp)
		{
			return new Occurrence(this,observationTimeStamp);
        }

        public bool RemoveOccurrences (DateTime date)
        {
            var removed = this.allOccurrences.RemoveAll (o => o.Date.Date == date.Date);
            return removed > 0;
        }

        public bool RemoveOccurrence (Occurrence toRemove)
        {
           return this.allOccurrences.Remove(toRemove);
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

        [JsonIgnore]
        protected TimeSpan ScheduledTargetTime
        {
            get
            {
                return   TargetTimeForDay(DateTime.Now.DayOfWeek);
            }
        }

        [JsonIgnore]
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
			return this.allOccurrences
				.OrderByDescending (o => o.TimeStamp)
				.Select (o => o.TimeStamp)
				.DefaultIfEmpty (ifNone)
				.FirstOrDefault ();
		}

        [JsonIgnore]
		public TimeSpan SinceLastOccurrence
		{
			get{
				var now = DateTime.Now;
				var mostRecent = this.MostRecentOccurrenceTimeStamp(now);
				return now - mostRecent;
			}
		}

        [JsonIgnore]
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

        [JsonIgnore]
		public DateTime TargetTimeToday
		{
			get
            {
                return (DateTime.Today + this.ScheduledTargetTime);
            }
		
		}

        [JsonIgnore]
        public bool IsMissed {
            get {
                return TargetTimePassed & !CompletedToday;
            }
        }

        [JsonIgnore]
        public bool IsSkipped {
            get {
                return this.allOccurrences.Any (o =>( o.IsSkipped == true) && (o.TimeStamp.Date == DateTime.Today));
            }
        }

        [JsonIgnore]
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

        [JsonIgnore]
		public bool TargetTimePassed 
		{
			get 
			{
				return this.ScheduledTargetTime < DateTime.Now.TimeOfDay;
			}
		}

        [JsonIgnore]
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

        [JsonIgnore]
        public Occurrence Earliest
        {
            get
            {
                var em= this.DistinctTimes.OrderBy(o => o.TotalMinutes).FirstOrDefault().TotalMinutes;
                return this.occurrences.OrderByDescending(o => o.TimeStamp).FirstOrDefault(o => o.Time.TotalMinutes == em);
            }
               
        }

        [JsonIgnore]
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

