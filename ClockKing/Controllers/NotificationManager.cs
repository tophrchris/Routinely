using System;
using Foundation;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using ClockKing.Extensions;
using ClockKing.Model;

namespace ClockKing
{
	public class NotificationManager
	{
		public UIApplication app{ get; set;}
		public NotificationManager()
		{
			this.app = UIApplication.SharedApplication;
		}


		public IEnumerable<UILocalNotification> ScheduledNotifications 
		{
			get
			{
				return UIApplication.SharedApplication.ScheduledLocalNotifications.AsEnumerable();
			}
		}
			
		public void EnsureNotifications(DataModel data,bool resetExisting=false)
		{
			
			if(resetExisting)
				app.CancelAllLocalNotifications ();

			var scheduledNotifications = this.ScheduledNotifications;

			var checkPoints = data.checkPoints.Values;

			var existingNotificationTitles = scheduledNotifications.Select (n => n.AlertTitle);
			var notificationsNeeded = checkPoints.Select (cp => cp.Name);

			var orphanedNotifications = existingNotificationTitles.Except (notificationsNeeded);

			var notificationTitlesToSkip = 
				checkPoints
					.Join(scheduledNotifications,
						cp=>cp.Name,
						n=>n.AlertTitle,
						(i,o)=>new{Checkpoint=i,Notification=o})
					.Where(j => j.Checkpoint.CompletedToday)
					.Where(j=>j.Checkpoint.TargetTimeUpcoming)
					.Where(j=>j.Notification.FireDate.ToDateTime().ToLocalTime().Date==DateTime.Today)
					.Select(j=>j.Checkpoint.Name);

			var TitlesToDelete = orphanedNotifications.Concat (notificationTitlesToSkip);

			var notificationsToDelete = 
				scheduledNotifications.Where(n=>TitlesToDelete.Contains(n.AlertTitle));

			try
			{
				foreach (var d in notificationsToDelete)
					app.CancelLocalNotification (d);

				var notificationsToCreate = 
					from checkpoint in checkPoints
						where !existingNotificationTitles.Contains(checkpoint.Name)
					select CreateNotificationForCheckpoint(checkpoint);
						
				foreach (var c in notificationsToCreate)
					app.ScheduleLocalNotification (c);
			}
			catch
			{
			
			}

		}

		public bool UpdateNotificationForCheckpoint(CheckPoint ToUpdate)
		{
			CancelNotificationforCheckpoint (ToUpdate);

			var created = CreateNotificationForCheckpoint (ToUpdate);
			app.ScheduleLocalNotification (created);

			return true;
		}
		public bool CancelNotificationforCheckpoint(CheckPoint ToCancel)
		{
			var NameToCancel = ToCancel.Name;

			if (this.ScheduledNotifications.Any(n=>n.AlertTitle==NameToCancel)) 
			{
				var found = this.ScheduledNotifications.Where(n=>n.AlertTitle==NameToCancel).Select(n=>n);
				foreach(var n in found)
					app.CancelLocalNotification (n);
				return true;
			}
			return false;
		}

		private UILocalNotification CreateNotificationForCheckpoint(CheckPoint toCreate)
		{
			var formatString = "It's time for {0}! on average, you complete this at {1}. Have you completed it yet?";

			var alarmTime = DateTime.Today.Add (toCreate.TargetTime);

			if (toCreate.TargetTime < DateTime.Now.TimeOfDay | toCreate.CompletedToday)
				alarmTime.AddDays (1);

			return new UILocalNotification () {
				FireDate = alarmTime.ToUniversalTime().ToNSDate(),
				SoundName = UILocalNotification.DefaultSoundName,
				AlertTitle = toCreate.Name,
				Category = "AddObservation",
				AlertBody = string.Format (formatString,
					toCreate.Name,
					(DateTime.Now.Date + toCreate.averageObservedTime).ToString ("t")),
				RepeatInterval = NSCalendarUnit.Day
			};
		}

		public static void HandleLocalNotification(UIApplication application, UILocalNotification notification)
		{

			var app = application.Delegate as AppDelegate;

			var opts = UIAlertController.Create(notification.AlertTitle,notification.AlertBody,UIAlertControllerStyle.ActionSheet);

			opts.AddAction(UIAlertAction.Create("Done!",UIAlertActionStyle.Default,
				(a)=>app.Controller.AddOccurrenceToCheckPoint(notification.AlertTitle,0)));

			opts.AddAction(UIAlertAction.Create("Snooze",UIAlertActionStyle.Default,
				(a)=>{
					notification.FireDate=DateTime.Now.AddMinutes(10).ToUniversalTime().ToNSDate();
					application.ScheduleLocalNotification(notification);
				}));

			opts.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel,null));

			app.Window.RootViewController.PresentViewController (opts, true, null);
		}

		public static bool HandleNotificationAction(UIApplication application,UILocalNotification localNotification, string actionIdentifier)
		{
			var data = new DataModel (false);
			var found = data.checkPoints [localNotification.AlertTitle]; 
			var actionBits = actionIdentifier.Split(':');
			var mins = int.Parse (actionBits [1]);	
			if (mins > 0) {
				localNotification.FireDate = DateTime.Now.AddMinutes (mins).ToUniversalTime ().ToNSDate ();
				localNotification.RepeatInterval = 0;
				application.ScheduleLocalNotification (localNotification);

			} else {
				var occ = found.CreateOccurrence (DateTime.Now.AddMinutes (mins));
				data.SaveOccurrence (occ);
				return true;
			}
			return false;
		}



		public UIUserNotificationCategory[] NotificationCategories
		{
			get{

				var offsets = from i in new Dictionary<int,string> ()
								{ 
									{ 10,"Snooze" },
									{0,"Just now!"},
									{ -15,"about {0} mins ago" }, 
									{ -30,"about {0} mins ago" }
								}
				               select new UIMutableUserNotificationAction ()
								{
									Identifier=string.Format("Add:{0}",i.Key),
									Title=string.Format(i.Value,Math.Abs((double)i.Key)),
									ActivationMode=UIUserNotificationActivationMode.Background,
									AuthenticationRequired=false,
									Destructive=i.Value=="Snooze",
									Behavior=UIUserNotificationActionBehavior.Default
								};

				var actions = offsets.ToArray ();
				var minimalActions = actions.Where (a => !a.Title.Contains ("about")).ToArray ();

				var addObservationCategory = new UIMutableUserNotificationCategory();
				addObservationCategory.Identifier="AddObservation";
				addObservationCategory.SetActions(minimalActions,UIUserNotificationActionContext.Minimal);
				addObservationCategory.SetActions(actions,UIUserNotificationActionContext.Default);

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

