using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;


namespace ClockKing.Core
{
    public class CheckPoint
    {
        private List<Occurrence> allOccurrences { get; set; } = new List<Occurrence> ();
        private List<ScheduledTargetTime> scheduledTargets { get; set; } = new List<ScheduledTargetTime> ();

        private Lazy<TimeSpan> EffectiveTargetTimeCalculator;
        private Lazy<TimeSpan> AverageCompletionTimeCalculator;
        private Lazy<DateTime> MostRecentOccurrenceCalculator;

        public Guid UniqueIdentifier { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Emoji { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Category { get; set; }
        public TimeSpan TargetTime {get; set; }
        public DateTime CreatedOn { get; set; }
        public RelativeTargetTime RelativeTarget { get; set; }

        public CheckPoint ()
        {
            ResetCalculators ();
        }

        private void ResetCalculators ()
        {
            this.EffectiveTargetTimeCalculator = new Lazy<TimeSpan> (() => EffectiveTargetTimeFor (DateTime.Now));
            this.AverageCompletionTimeCalculator = new Lazy<TimeSpan> (() => CalculateAverageCompletionTime ());
            this.MostRecentOccurrenceCalculator = new Lazy<DateTime> (() =>MostRecentOccurrenceTimeStamp(DateTime.Now));
        }

        #region occurrence management
        [JsonIgnore]
        public IEnumerable<Occurrence> AllOccurrences { get { return this.allOccurrences; } }

        private IEnumerable<Occurrence> occurrences {
            get {
                return allOccurrences.Where (o => !o.IsSkipped);
            }
        }
        [JsonIgnore]
        public IEnumerable<Occurrence> Occurrences { get { return this.occurrences; } }



        public void AddOccurrence (Occurrence newOccurance)
        {
            this.allOccurrences.Add (newOccurance);
            this.ResetCalculators ();
        }
        public Occurrence CreateOccurrence ()
        {
            return this.CreateOccurrence (DateTime.Now.ToLocalTime ());
        }

        public Occurrence CreateOccurrence (DateTime observationTimeStamp)
        {
            return new Occurrence (this, observationTimeStamp);
        }

        public bool RemoveOccurrences (DateTime date)
        {
            var removed = this.allOccurrences.RemoveAll (o => o.Date.Date == date.Date);
            this.ResetCalculators ();
            return removed > 0;
        }

        public bool RemoveOccurrence (Occurrence toRemove)
        {
            this.ResetCalculators ();
            return this.allOccurrences.Remove (toRemove);
        }
        #endregion

        #region TargetTime Resolution

        public TimeSpan TargetTimeForDay (DayOfWeek day) => EffectiveTargetTimeFor(day);

        [JsonIgnore]
        protected TimeSpan EffectiveTargetTime {
            get 
            {
                if (this.RelativeTarget == null)
                    return EffectiveTargetTimeCalculator.Value;
                else
                    return EffectiveTargetTimeFor (DateTime.Now);
            }
        }

        public TimeSpan EffectiveTargetTimeFor(DateTime d) 
        {
            if (RelativeTarget != null)
                return RelativeTarget.OffsetScheduledTimeForDate(d);
            else
                return EffectiveTargetTimeFor(d.DayOfWeek);
        }
        public TimeSpan EffectiveTargetTimeFor(DayOfWeek day) 
        {
            if (scheduledTargets.Any ()) {
                var f = this.scheduledTargets
                    .FirstOrDefault (t => t.ApplicableDays.Contains (day));

                return f?.TargetTime ?? this.TargetTime;

            } else
                return TargetTime;
            
        }

        #endregion

        #region scheduledTarget management
        public IEnumerable<ScheduledTargetTime> ScheduledTargets { get { return this.scheduledTargets; } }

        public ScheduledTargetTime AddScheduledtarget (TimeSpan? scheduledTargetTime, List<DayOfWeek> days)
        {
            var alt = new ScheduledTargetTime () {
                TargetTime = scheduledTargetTime,
                ApplicableDays = days.ToArray ()
            };

            this.scheduledTargets.Add (alt);
            return alt;
        }

        public bool RemoveScheduledTarget (ScheduledTargetTime toRemove)
        {
            return this.scheduledTargets.Remove (toRemove);
        }


        [JsonIgnore]
        public bool IsActive {
            get {
                return IsActiveForDay (DateTime.Today.DayOfWeek);
            }

        }

        [JsonIgnore]
        public DateTime TargetTimeToday {
            get {
                return (DateTime.Today + this.EffectiveTargetTime);
            }

        }

        public bool IsActiveForDay (DayOfWeek day)
        {
            var relevantAlternatives = this.scheduledTargets.Where (t => t.ApplicableDays.Contains (day));
            if (relevantAlternatives.Any (t => !t.TargetTime.HasValue))
                return false;

            return true;
        }
       

        #endregion

        #region basic metrics and calculated members
		public TimeSpan AverageCompletionTime
		{
			get
            {
                return AverageCompletionTimeCalculator.Value;
			}
		}

        private TimeSpan CalculateAverageCompletionTime ()
        {
            if (!occurrences.Any ())
                return this.EffectiveTargetTime;

            var avgminutes = this.occurrences.Average (o => o.Time.TotalMinutes);
            return TimeSpan.FromMinutes (avgminutes);
        }
            

		public DateTime MostRecentOccurrenceTimeStamp()
        {
            return MostRecentOccurrenceCalculator.Value;
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

        //TODO: re-write this to be schedule target time aware
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

                return this.occurrences.Any (o => o.Date.Date == DateTime.Today);
			}
		}

        [JsonIgnore]
		public bool TargetTimePassed 
		{
			get 
			{
				return this.EffectiveTargetTime < DateTime.Now.TimeOfDay;
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
        #endregion

		public override string ToString ()
		{
			return string.Format ("{3}{0}: target={1}, avg={2}", 
				this.Name,
				this.TargetTime,
				this.AverageCompletionTime,
				this.Emoji);
		}
	}

    public class ScheduledTargetTime
    {
        public TimeSpan? TargetTime{ get; set; }
        public DayOfWeek[] ApplicableDays{ get; set; }
    }

    public class RelativeTargetTime
    {
        private Guid relatedGuid;
        public Guid RelatedCheckPointGuid {
            get 
            {
                if (RelatedCheckPoint == null)
                    return relatedGuid;
                else
                    return RelatedCheckPoint.UniqueIdentifier;
            }
            set 
            { 
                relatedGuid = value;
            } 
        }

        [JsonIgnore]
        public CheckPoint RelatedCheckPoint { get; set; }

        public TimeSpan Offset { get; set;}

        public TimeSpan OffsetScheduledTimeForDate(DateTime d)
        {
            var q = from oc in RelatedCheckPoint.Occurrences
                    where oc.Date == d.Date
                    orderby oc.TimeStamp descending
                    select oc.Time;
            var f = q.DefaultIfEmpty (RelatedCheckPoint.EffectiveTargetTimeFor(d))
                     .FirstOrDefault ();
            return f.Add (Offset);
        }
        public bool IsActiveOnDate (DateTime d)
        {
            return this.RelatedCheckPoint.Occurrences.Any (o => o.Date == d.Date);
        }
        public bool isActive { get { return IsActiveOnDate (DateTime.Now);}}

    }
}

