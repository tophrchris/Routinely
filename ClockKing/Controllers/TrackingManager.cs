using System;
using Google.Analytics;
using System.Diagnostics;

namespace ClockKing
{
	public class TrackingManager
	{
		
		private static ITracker tracking { get; set; }

		public static void Track(string screenName)
		{
			if (!ClockKingOptions.EnableAnalyticsTracking)
				return;
			EnsureTracking();
			tracking.Set(GaiConstants.ScreenName, screenName);
			Debug.WriteLine("on screen:" + screenName);
			tracking.Send(DictionaryBuilder.CreateScreenView().Build());
		}

		public static void Track(string category, string action, string label, int value = 0)
		{
			if (!ClockKingOptions.EnableAnalyticsTracking)
				return;
			EnsureTracking();
			Debug.WriteLine(string.Format("{0}:{1}:{2}", category, action, label));
			tracking.Send(DictionaryBuilder.CreateEvent(category, action, label, value).Build());
		}

		public static void EnsureTracking()
		{
			if (tracking == null)
			{
				Gai.SharedInstance.DispatchInterval = 20;
				Gai.SharedInstance.TrackUncaughtExceptions = true;
				tracking =  Gai.SharedInstance.GetTracker("UA-82950326-1");
			}
		}
	}
}

