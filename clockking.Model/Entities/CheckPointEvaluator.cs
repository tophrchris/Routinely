using System;
using System.Linq;
using System.Collections.Generic;
using Humanizer;
using ClockKing.Extensions;

namespace ClockKing.Core
{
    public class CheckPointEvaluator
    {
        public int OnTimeStreak { get; private set;}
        public int CompletionStreak { get; private set;}
        public int TotalActiveDays { get; private set;}
        public int DaysCompleted { get; private set; }
        public int OnTimeCompletions { get; private set; }
        public int ActiveDaysSinceMostRecentCompletion { get; private set; }
        public int CalendarDaysSinceCreation{ get; private set; }
        public TimeSpan AccuracyOfMostRecentOccurrence { get; set;}

        private CheckPoint checkpoint{ get; set;}

        public CheckPointEvaluator(CheckPoint toEvaluate)
        {
            this.checkpoint = toEvaluate;
            this.CalendarDaysSinceCreation  = (int)(DateTime.Now - checkpoint.CreatedOn).TotalDays;
            this.TotalActiveDays = To(checkpoint.CreatedOn, checkpoint, DateTime.Now).Count();
            this.ActiveDaysSinceMostRecentCompletion = To(checkpoint.MostRecentOccurrenceTimeStamp(), checkpoint, DateTime.Now).Count();

            this.DaysCompleted = checkpoint.Occurrences
                .Where(o=>checkpoint.ActiveForDay(o.Date.DayOfWeek))
                .Select(o => o.Date)
                .Distinct()
                .Count();

            this.OnTimeCompletions = checkpoint.Occurrences
                .Where(o => Math.Abs(o.MinutesFromTarget) <= 15)
                .Select(o => o.Date)
                .Distinct()
                .Count();

            this.CompletionStreak = this.StreakDetector(checkpoint);
            this.OnTimeStreak = this.PerfectStreakDector(checkpoint);

            if (checkpoint.Occurrences.Any()) 
            {
                var mostRecent = checkpoint.MostRecentOccurrenceTimeStamp ();
                var accuracy = mostRecent - checkpoint.TargetTimeForDay (mostRecent.DayOfWeek);
            }


        }

        public string Motivation 
        {
            get 
            {
                if(checkpoint.CompletedToday)
                if (Math.Abs(AccuracyOfMostRecentOccurrence.TotalMinutes) <= 5)
                        return "\U0001F396 On Time Completion! ";

                return string.Empty;
            }
        }


        public string Evaluation
        {
            get
            {
                if (this.OnTimeStreak > 2)
                    return "You're on a {0} day on-time streak! great work!".FormatWith(this.OnTimeStreak);
                if (this.CompletionStreak > 2)
                    return "You've completed this goal {0} days in a row!".FormatWith(this.CompletionStreak);

                if (TotalActiveDays > 2)
                {
                    var com = (float)DaysCompleted / (float)TotalActiveDays;
                    if (com > .7f)
                        return "You've completed this goal {0:P0} of the last {1} days!".FormatWith(com, CalendarDaysSinceCreation);
                }
                if (checkpoint.TargetTime < DateTime.Now.TimeOfDay)
                {
                    if (this.OnTimeStreak > 0)
                        return "You've been on time for the last {0} days.  Complete on time today to keep your streak going!".FormatWith(OnTimeStreak);

                    if (this.CompletionStreak > 0)
                        return "You're on a {0} day streak- complete today to keep the streak alive!".FormatWith(CompletionStreak);

                    return "Start an on-time streak: complete today between {0} and {1}!".FormatWith(checkpoint.TargetTime.Subtract(15.Minutes()).ToAMPMString(), checkpoint.TargetTime.Add(15.Minutes()).ToAMPMString());
                }
                else
                {
                    return "It's been {0} days since you've last completed this goal. Get back on track!".FormatWith(ActiveDaysSinceMostRecentCompletion);
                }
            }
        }
            
        private IEnumerable<DateTime> To(DateTime start, CheckPoint checkpoint, DateTime end)
        {
            var current = start.Date;

            while (current < end.Date)
            {
                if (checkpoint.ActiveForDay(current.Date.DayOfWeek))
                    yield return current;
                current = current.AddDays(1);
            }
            yield break;

        }

        private int PerfectStreakDector(CheckPoint checkpoint)
        {
            return StreakDetector(checkpoint, checkpoint.Occurrences.Where(o => Math.Abs(o.MinutesFromTarget) <= 15).Select(o => o.Date));
        }

        private int StreakDetector(CheckPoint checkpoint)
        {
            return StreakDetector(checkpoint, checkpoint.Occurrences.Select(o => o.Date));
        }
        //TODO: this should return a list of dates, so that we can either count or first/last them
        private int StreakDetector(CheckPoint checkpoint,IEnumerable<DateTime> occurrenceDates)
        {
            var current = DateTime.Now.Date;
            var streak = 0;
            var effectiveDates= occurrenceDates.Distinct().ToList();
            while (current >= checkpoint.CreatedOn)
            {
                if (checkpoint.ActiveForDay(current.DayOfWeek))
                {
                    if (!effectiveDates.Contains(current))
                        break;
                    else
                        streak += 1;
                }
                current = current.AddDays(-1);
            }
            return streak;
        }
    }
}

