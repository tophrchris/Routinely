using System;
using CoreSpotlight;
using ClockKing.Core;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using System.Diagnostics;

namespace ClockKing
{
	public class SpotlightManager
	{
		private static string Domain = "org.Hollanders.ClockKing.Goals";
		public static void PresentGoalsForIndexing(Dictionary<string,CheckPoint> checkpoints )
		{
			
			var items = checkpoints.Select(kv =>
			{
				var a = new CSSearchableItemAttributeSet(itemContentType: "");
				a.Title = kv.Key;
				a.ContentDescription = string.Format("last completed on {0}", kv.Value.MostRecentOccurrenceTimeStamp().ToString("G"));
				a.TextContent = kv.Value.ToString();
				var item = new CSSearchableItem(kv.Value.UniqueIdentifier.ToString(), Domain, a);
				return item;
			});

			var errorHandler = new Action<NSError, string>((error, success) =>
			{
				if (error != null)
					Debug.WriteLine(error);
			});

			try
			{
				CSSearchableIndex.DefaultSearchableIndex.DeleteWithDomain(new string[] { Domain }, (error) =>errorHandler(error,"Index Items Deleted"));
				CSSearchableIndex.DefaultSearchableIndex.Index(items.ToArray<CSSearchableItem>(), (error)=>errorHandler(error,"Indexed items successfully"));
			}
			catch(Exception e) { Debug.WriteLine(e.Message); }
		}

		public static void HandleUserActivity(UIApplication application, NSUserActivity activity)
		{
			var del = application.Delegate as AppDelegate;

			if (del.CheckPointData.checkPoints.ContainsKey(activity.Title))
			{
				var found = del.CheckPointData.checkPoints[activity.Title];
				del.LaunchActions.Enqueue((c) => c.ShowDetailDialogFor(found));
			}

		}
	}
}


