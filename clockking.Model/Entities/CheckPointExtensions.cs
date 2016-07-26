using System;
using Humanizer;
using Humanizer.Configuration;
using Humanizer.DateTimeHumanizeStrategy;
using System.Linq;

namespace ClockKing.Core
{
    public static class CheckPointExtensions
    {
        public static string GetProgress (this CheckPoint cp)
        {
             Configurator.DateTimeHumanizeStrategy = new PrecisionDateTimeHumanizeStrategy (.9D);

            if (!cp.Occurrences.Any ())
                return "created {0}".FormatWith (cp.CreatedOn.ToLocalTime().Humanize (false)).AsSentence ();

            if (!cp.Active | !cp.Enabled) 
                return "last completed {0}".FormatWith(cp.MostRecentOccurrenceTimeStamp ().Humanize ()).AsSentence();


            if (cp.CompletedToday) 
            {
                var precision = cp.SinceLastOccurrence.TotalMinutes > 1 ? 2 : 1;
                return "{0} ago".FormatWith (cp.SinceLastOccurrence.Humanize(precision)).AsSentence();
            }

            if (cp.IsMissed | cp.IsSoon())
                return cp.TargetTimeToday.ToUniversalTime ().Humanize().AsSentence();

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

