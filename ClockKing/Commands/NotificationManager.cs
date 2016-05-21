using System;
using Foundation;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using ClockKing.Extensions;

namespace ClockKing
{
	public class NotificationManager
	{
		private CommandManager commandManager{ get; set;}
		public NotificationManager (CommandManager commandMgr)
		{
			this.commandManager = commandMgr;
		}

		public UILocalNotification[] ScheduledNotifications 
		{
			get
			{
				return UIApplication.SharedApplication.ScheduledLocalNotifications;
			}
		}


		public void EnsureNotifications(DataModel data)
		{

			var app = UIApplication.SharedApplication;
			var scheduledNotifications = this.ScheduledNotifications;

			var checkPoints = data.CheckPointPairs.Select (p => p.firstEvent);

			var existingNotificationTitles = scheduledNotifications.Select (n => n.AlertTitle).ToList ();
			var notificationsNeeded = checkPoints.Select (cp => cp.Name);

			var notificationTitlesToDelete = existingNotificationTitles.Except (notificationsNeeded);
			var notificationsToDelete = scheduledNotifications.Where(n=>notificationTitlesToDelete.Contains(n.AlertTitle));

			foreach (var d in notificationsToDelete)
				app.CancelLocalNotification (d);

			var formatString = "It's time for {0}! on average, you complete this at {1}. Have you completed it yet?";

			var notificationsToCreate = 
				from checkpoint in checkPoints
				where !existingNotificationTitles.Contains(checkpoint.Name)
				select new UILocalNotification () 
				{
				FireDate= DateTime.Today
								.AddDays((checkpoint.TargetTime>DateTime.Now.TimeOfDay)?0:1)
								.Add(checkpoint.TargetTime)
								.ToUniversalTime()
								.ToNSDate(),
				AlertTitle=checkpoint.Name,
				Category="AddObservation",
				AlertBody=string.Format(formatString,
					checkpoint.Name,
					DateTime.Now.Day+checkpoint.averageObservedTime.ToString("t")),
				RepeatInterval=NSCalendarUnit.Day
				};
					
			foreach (var c in notificationsToCreate)
				app.ScheduleLocalNotification (c);

		}


		public UIUserNotificationCategory[] NotificationCategories
		{
			get{

				var offsets = from i in new Dictionary<int,string> ()
								{ { -15,"15 mins ago" }, { 0,"Just now!" }, { 15,"in 15 mins" } }
				               select new UIMutableUserNotificationAction ()
								{
									Identifier=string.Format("Add:{0}",i.Key),
									Title=i.Value,
									ActivationMode=UIUserNotificationActivationMode.Background,
									AuthenticationRequired=false,
									Destructive=false,
									
									Behavior=UIUserNotificationActionBehavior.Default
								};
						

				var addObservationCategory = new UIMutableUserNotificationCategory();
				addObservationCategory.Identifier="AddObservation";
				addObservationCategory.SetActions(offsets.ToArray(),UIUserNotificationActionContext.Default);

				return new[]{ addObservationCategory };
			}
		}

		public bool EnsureSettings(UIApplication app)
		{
			var categories = this.NotificationCategories;

			var settings = UIUserNotificationSettings.GetSettingsForTypes(
				UIUserNotificationType.Alert |
				UIUserNotificationType.Badge | 
				UIUserNotificationType.Sound
				,new NSSet(categories));

			app.RegisterUserNotificationSettings (settings);

			return true;
		}

	}
}

