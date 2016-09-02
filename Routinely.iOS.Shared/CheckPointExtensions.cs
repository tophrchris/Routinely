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


namespace ClockKing.Core.Shared
{
	public static class CheckPointExtensions
	{
		public static string GetProgress(this CheckPoint cp, bool preferDue=false )
		{
			Configurator.DateTimeHumanizeStrategy = new PrecisionDateTimeHumanizeStrategy(.9D);


			if (!cp.IsActive | !cp.IsEnabled)
				return "last completed {0}".FormatWith(cp.MostRecentOccurrenceTimeStamp().ToUniversalTime().Humanize()).AsSentence();


			if (cp.CompletedToday | cp.IsSkipped)
			{
				var precision = cp.SinceLastOccurrence.TotalMinutes > 1 ? 2 : 1;
				var action = cp.IsSkipped ? "Skipped " : "";
				return "{1}{0} ago".FormatWith(cp.SinceLastOccurrence.Humanize(precision), action).AsSentence();
			}


			if (cp.IsMissed | cp.IsSoon() | preferDue)
				return (cp.IsMissed?"Missed ":"Due ")+ cp.TargetTimeToday.ToUniversalTime().Humanize().AsSentence();

			if (!cp.Occurrences.Any())
				return "created {0}".FormatWith(cp.CreatedOn.ToLocalTime().Humanize(false)).AsSentence();
			

			return "completed {0} times".FormatWith(cp.Occurrences.Count()).AsSentence();

		}

		public static bool IsSoon(this CheckPoint cp, int mins = 90)
		{
			return !(cp.CompletedToday | cp.IsMissed) & cp.TargetTimeToday <= DateTime.Now.AddMinutes(mins);
		}

		public static UILocalNotification GetMotivationalNotification(this CheckPoint toCreate)
		{
			var eval = new CheckPointEvaluator(toCreate);
			if (eval.Motivation != string.Empty)
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

	}
}

