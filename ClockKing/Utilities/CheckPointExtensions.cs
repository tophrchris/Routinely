using System;
using Humanizer;
using Humanizer.Configuration;
using Humanizer.DateTimeHumanizeStrategy;
using System.Linq;
using ClockKing.Core;

namespace ClockKing
{
    public static class CheckPointExtensions
    {
		public static TableCellRefresher.RefreshRate GetDesiredRefreshRate(this CheckPoint cp)
		{

			var since = DateTime.Now - cp.MostRecentOccurrenceTimeStamp(cp.CreatedOn);
			var soon = cp.IsSoon(5);

			if (since.TotalSeconds < 30)
				return TableCellRefresher.RefreshRate.Instant;
			else
			{
				if (since.TotalMinutes < 2 | soon)
					return TableCellRefresher.RefreshRate.Fast;
				else
					if (since.TotalMinutes < 15)
					return TableCellRefresher.RefreshRate.Standard;
				else
					return TableCellRefresher.RefreshRate.Slow;
			}

		}
        public static string GetProgress (this CheckPoint cp)
        {
             Configurator.DateTimeHumanizeStrategy = new PrecisionDateTimeHumanizeStrategy (.9D);


            if (cp.IsMissed | cp.IsSoon ())
                return cp.TargetTimeToday.ToUniversalTime ().Humanize ().AsSentence ();
            
            if (!cp.Occurrences.Any ())
                return "created {0}".FormatWith (cp.CreatedOn.ToLocalTime().Humanize (false)).AsSentence ();

            if (!cp.Active | !cp.Enabled) 
				return "last completed {0}".FormatWith(cp.MostRecentOccurrenceTimeStamp ().ToUniversalTime().Humanize ()).AsSentence();


            if (cp.CompletedToday | cp.IsSkipped) 
            {
                var precision = cp.SinceLastOccurrence.TotalMinutes > 1 ? 2 : 1;
				var action = cp.IsSkipped ? "Skipped " : "";
				return "{1}{0} ago".FormatWith (cp.SinceLastOccurrence.Humanize(precision),action).AsSentence();
            }


            return "completed {0} times".FormatWith (cp.Occurrences.Count ()).AsSentence();

        }

        public static bool IsSoon (this CheckPoint cp, int mins = 90)
        {
            return !(cp.CompletedToday|cp.IsMissed) & cp.TargetTimeToday <= DateTime.Now.AddMinutes (mins);
        }
        public static string AsSentence (this string passage)
        {
            var endings = new [] { ".", "!", "?", ":" };
            if (!endings.Any(e=>passage.EndsWith (e,StringComparison.CurrentCulture)))
                passage += ".";
            return passage.ApplyCase (LetterCasing.Sentence);
        }
    }
}

