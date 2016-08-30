using System;
using MTiRate;
using UIKit;

namespace ClockKing
{
	public class RatingsManager
	{
		public static void ConfigureRatingsPrompt()
		{
			iRate.SharedInstance.PreviewMode = false;
			iRate.SharedInstance.DaysUntilPrompt = 3;
			iRate.SharedInstance.UsesUntilPrompt = 15;
			iRate.SharedInstance.RemindPeriod = 3;
			iRate.SharedInstance.MessageTitle = "Please Rate Routinely!";
			iRate.SharedInstance.Message = "I hope that Routinely has made it easier to be more mindful of your daily Routine. If so, please take a moment to leave a rating and review on the App Store. Thanks!"; ;
			iRate.SharedInstance.CancelButtonLabel = "No Thanks.";
			iRate.SharedInstance.RemindButtonLabel = "Maybe Later?";
			iRate.SharedInstance.RateButtonLabel = "Rate It Now!";


			AppDelegate instance = null;

			if (UIApplication.SharedApplication != null)
				instance = UIApplication.SharedApplication.Delegate as AppDelegate;

			var logger = new Action<string>((msg) =>
			{
				if (instance != null)
					instance.LogEvent("Rating", msg, "n/a");

				Console.WriteLine("Rating:" + msg);
			});

			iRate.SharedInstance.UserDidAttemptToRateApp += (sender, e) =>
			{
				logger("Attempted");
			};

			iRate.SharedInstance.UserDidDeclineToRateApp += (sender, e) =>
			{
				logger("Declined");
			};

			iRate.SharedInstance.UserDidRequestReminderToRateApp += (sender, e) =>
			{
				logger("Snoozed");
			};
		}
		public static void Prompt()
		{
			iRate.SharedInstance.PromptIfNetworkAvailable();
		}
	}
}

