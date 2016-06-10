using System;
using System.Linq;
using System.Collections.Generic;

namespace ClockKing.Core
{
    public class CheckPointEvaluator
    {


        private CheckPoint checkpoint{ get; set;}

        public CheckPointEvaluator(CheckPoint toEvaluate)
        {
            this.checkpoint = toEvaluate;
        }

        public string Evaluation
        {
            get
            {
                var daysSinceCreation = (int)(DateTime.Now - checkpoint.CreatedOn).TotalDays;
                var ActiveDaysSinceCreation = To(checkpoint.CreatedOn, checkpoint, DateTime.Now).Count();
                var sinceMostRecent = To(checkpoint.MostRecentOccurrenceTimeStamp(), checkpoint, DateTime.Now).Count();

                var distinctOccurrenceDays = checkpoint.Occurrences
                    .Where(o=>checkpoint.ActiveForDay(o.Date.DayOfWeek))
                    .Select(o => o.Date)
                    .Distinct()
                    .Count();

                var perfectDays = checkpoint.Occurrences
                                        .Where(o => Math.Abs(o.MinutesFromTarget) <= 15)
                                        .Select(o => o.Date)
                                        .Distinct()
                                        .Count();

                var streak = this.StreakDetector(checkpoint);
                var perfectStreak = this.PerfectStreakDector(checkpoint);

                return string.Format("completed {3} on {0} out of {1}({4}) days, with {2} perfect days and a streak of {5}: perfect streak of {6}. {7} days since last.",
                    distinctOccurrenceDays,
                    daysSinceCreation,
                    perfectDays,
                    checkpoint.Name,
                ActiveDaysSinceCreation,
                streak,
                perfectStreak,
                sinceMostRecent);
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

