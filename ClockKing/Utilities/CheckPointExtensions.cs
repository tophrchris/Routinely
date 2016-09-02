using System;
using System.Linq;
using ClockKing.Core;
using System.Collections.Generic;
using Foundation;
using UIKit;
using ClockKing.Extensions;
using ClockKing.Core.Shared;


namespace ClockKing
{
    public  static class CheckPointExtensions
    {
		public static TableCellRefresher.RefreshRate GetDesiredRefreshRate(this CheckPoint cp)
		{

			//if new or recently created, since will be relative small.
			var since = DateTime.Now - cp.MostRecentOccurrenceTimeStamp (cp.CreatedOn);

			//until will be positive if upcoming, negative if missed;
			var until = cp.TargetTimeToday - DateTime.Now;
			var missed = cp.IsMissed;

			var justCompleted = since.TotalSeconds <= 30;
			var RecentlyCompleted = since.TotalMinutes <= 2;


			var justMissed = missed &&
			                   (until.TotalSeconds < 0) && (until.TotalSeconds >= -30);
			var recentlyMissed = missed &&
		                       (until.TotalSeconds < 0) && (until.TotalMinutes >= -2);

			var soon = cp.IsSoon (5);
			var reallySoon = cp.IsSoon (1);

			if (reallySoon|justCompleted|justMissed)
				return TableCellRefresher.RefreshRate.Instant;
			
			if (soon|RecentlyCompleted|recentlyMissed)
				return TableCellRefresher.RefreshRate.Fast;

			if (!(cp.IsEnabled && cp.IsActive))
				return TableCellRefresher.RefreshRate.Slow;

			return TableCellRefresher.RefreshRate.Standard;
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

