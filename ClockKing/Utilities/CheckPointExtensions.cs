using System;
using Humanizer;
using Humanizer.Configuration;
using Humanizer.DateTimeHumanizeStrategy;
using System.Linq;
using ClockKing.Core;
using System.Collections.Generic;
using Foundation;
using UIKit;
using ClockKing.Extensions;


namespace ClockKing
{
    public static class CheckPointExtensions
    {
		public static TableCellRefresher.RefreshRate GetDesiredRefreshRate(this CheckPoint cp)
		{

			var since = DateTime.Now - cp.MostRecentOccurrenceTimeStamp(cp.CreatedOn);
			var soon = cp.IsSoon(5);
			var reallySoon = cp.IsSoon (1);

			if (Math.Abs(since.TotalSeconds) < 30 | reallySoon)
				return TableCellRefresher.RefreshRate.Instant;
			else
			{
				if (Math.Abs(since.TotalMinutes) < 2 | soon)
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

            if (!cp.IsActive | !cp.IsEnabled) 
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

		public static IEnumerable<UILocalNotification> RequiredNotifications(this CheckPoint toCreate)
		{
			var alerts = new List<UILocalNotification>();

			var formatString = "It's time for {0}! on average, you complete this at {1}. Have you completed it yet?";
			var alertBody = string.Format(formatString,
										  toCreate.Name, (DateTime.Now.Date + toCreate.AverageCompletionTime).ToString("t"));

			if (toCreate.ScheduledTargets.Any())
			{
				foreach (var st in toCreate.ScheduledTargets.Where(st => st.TargetTime.HasValue))
				{
					foreach (var d in st.ApplicableDays)
					{
						DateTime alertDate = DetermineNextAlertTimeStamp(d, toCreate);

						alerts.Add(alertFromCheckPoint(toCreate, alertBody, alertDate));
					}
				}
				var altDays = toCreate.ScheduledTargets.SelectMany(st => st.ApplicableDays);
				var allDays = new List<DayOfWeek>()
				{ 		   DayOfWeek.Sunday,
						   DayOfWeek.Monday,
						   DayOfWeek.Tuesday,
						   DayOfWeek.Wednesday,
						   DayOfWeek.Thursday,
						   DayOfWeek.Friday,
						   DayOfWeek.Saturday};
				var remainingDays = allDays.Except(altDays);
				foreach (var d in remainingDays)
				{
					var alertDate = DetermineNextAlertTimeStamp(d,toCreate);
					alerts.Add(alertFromCheckPoint(toCreate, alertBody, alertDate));
				}
			}
			else {

				var alarmTime = DateTime.Today.Add(toCreate.TargetTimeToday.TimeOfDay);

				if (toCreate.TargetTimeToday < DateTime.Now | toCreate.CompletedToday)
					alarmTime = alarmTime.AddDays(1);

				alerts.Add(alertFromCheckPoint(toCreate, alertBody, alarmTime));
			}

			return alerts;
		}

		private static UILocalNotification alertFromCheckPoint(CheckPoint toCreate, string alertBody, DateTime alarmTime)
		{
			
				var alert = new UILocalNotification()
				{
					FireDate = alarmTime.ToUniversalTime().ToNSDate(),
					SoundName = UILocalNotification.DefaultSoundName,
					AlertTitle = toCreate.Name,
					Category = "AddObservation",
					AlertBody = alertBody,
					RepeatInterval = NSCalendarUnit.Day
				};
				return alert;

		}

		public static UILocalNotification GetMotivationalNotification(this CheckPoint toCreate)
		{
			var eval = new CheckPointEvaluator(toCreate);
			if (eval.Motivation !=string.Empty)
			{
				
				var alert = new UILocalNotification()
				{
					SoundName = UILocalNotification.DefaultSoundName,
					AlertTitle = toCreate.Name,
					Category = "Motivation",
					AlertBody = eval.Motivation + eval.Evaluation
				};
				return alert;
			}
			return null;
		}



		static DateTime DetermineNextAlertTimeStamp(DayOfWeek d, CheckPoint toAlert)
		{
			var DaysFromNow = d - DateTime.Today.DayOfWeek;

			if (DaysFromNow < 0)
				DaysFromNow += 7;

			if (DaysFromNow == 0 && ((toAlert.IsMissed)|toAlert.CompletedToday))
				DaysFromNow += 7;

			var alertDate = DateTime.Today.Date.AddDays(DaysFromNow);
			var alertTime = alertDate.Add(toAlert.EffectiveTargetTimeFor(alertDate));
			return alertTime;
		}
}
}

