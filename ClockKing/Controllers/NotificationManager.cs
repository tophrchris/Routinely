using System;
using Foundation;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using ClockKing.Extensions;
using ClockKing.Core;
using System.Diagnostics;

namespace ClockKing
{
	public class NotificationManager
	{
		public UIApplication app{ get; set;} = UIApplication.SharedApplication;


		public IEnumerable<UILocalNotification> ScheduledNotifications 
		{
			get
			{
				return UIApplication.SharedApplication.ScheduledLocalNotifications.AsEnumerable();
			}
		}
			
		public void EnsureNotifications(DataModel data,bool resetExisting=true)
		{
			var checkPoints = data.checkPoints.Values;

			var required = checkPoints
								.Where(cp=>cp.Enabled)
								.SelectMany(cp => cp.RequiredNotifications())
			                    .OrderBy(n => n.FireDate.ToDateTime());

			if (resetExisting)
				app.CancelAllLocalNotifications();

			foreach (var ln in required)
			{
				Debug.WriteLine(string.Format("scheduling {0} at {1}", ln.AlertTitle, ln.FireDate.ToDateTime().ToLocalTime()));
				app.ScheduleLocalNotification(ln);
			}
		}



		public static void HandleLocalNotification(UIApplication application, UILocalNotification notification)
		{

			if (notification.Category == "Motivation")
				return;
			
			var app = application.Delegate as AppDelegate;
			var cpm = app.Controller.CheckPoints;

			var opts = UIAlertController.Create(notification.AlertTitle,notification.AlertBody,UIAlertControllerStyle.ActionSheet);

			opts.AddAction(UIAlertAction.Create("Done!",UIAlertActionStyle.Default,
				(a)=>cpm.AddOccurrenceToCheckPoint(notification.AlertTitle,0)));

			opts.AddAction(UIAlertAction.Create("Snooze",UIAlertActionStyle.Default,
				(a)=>{
					var nn = notification;
					nn.FireDate=DateTime.Now.AddMinutes(10).ToUniversalTime().ToNSDate();
					nn.RepeatInterval=0;
					application.ScheduleLocalNotification(nn);
				}));

			opts.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel,null));

		
			app.Window.RootViewController.PresentViewController (opts, true, null);
			app.Controller.RespondToModelChanges();

		}

		public static void PresentMotivationalNotification(CheckPoint source)
		{
			var alert = source.GetMotivationalNotification();
			if(alert!=null)
				UIApplication.SharedApplication.PresentLocalNotificationNow(alert);
		}

		public static bool HandleNotificationAction(UIApplication application,UILocalNotification localNotification, string actionIdentifier)
		{
			var data = new DataModel (AppDelegate.DefaultDataProvider,true);

			var found = data.checkPoints [localNotification.AlertTitle]; 
			var actionBits = actionIdentifier.Split(':');
			var mins = int.Parse (actionBits [1]);	
			if (mins >0) {
				if (mins == 2)
				{
					var skip = found.CreateOccurrence();
					skip.IsSkipped = true;
					data.SaveOccurrence(skip);
					return true;
				}
				var nn = localNotification;
				nn.FireDate = DateTime.Now.AddMinutes (mins).ToUniversalTime ().ToNSDate ();
				nn.Category = "AfterSnooze";
				nn.RepeatInterval = 0;
				application.ScheduleLocalNotification (nn);
			} else {

				var occ = found.CreateOccurrence(DateTime.Now.AddMinutes(mins));
				data.SaveOccurrence(occ);
				found.AddOccurrence(occ);
				PresentMotivationalNotification(found);

				return true;
			}
			return false;
		}


		public UIUserNotificationCategory[] NotificationCategories
		{
			get
			{
				return new[]{ this.AddOccurrenceCategory,this.AfterSnoozeCategory };
			}
		}

		private UIMutableUserNotificationCategory AddOccurrenceCategory
		{
			get
			{
				var offsets = from i in new Dictionary<int, string>()
								{
									{ 10,"Snooze" },
									{0,"Just now!"},
									{ -15,"about {0} mins ago" },
									{ -30,"about {0} mins ago" }
								}
							  select new UIMutableUserNotificationAction()
							  {
								  Identifier = string.Format("Add:{0}", i.Key),
								  Title = string.Format(i.Value, Math.Abs((double)i.Key)),
								  ActivationMode = UIUserNotificationActivationMode.Background,
								  AuthenticationRequired = false,
								  Destructive = i.Value == "Snooze",
								  Behavior = UIUserNotificationActionBehavior.Default
							  };

				var actions = offsets.ToArray();
				var minimalActions = actions.Where(a => !a.Title.Contains("about")).ToArray();

				var addOccurrenceCategory = new UIMutableUserNotificationCategory();
				addOccurrenceCategory.Identifier = "AddObservation";
				addOccurrenceCategory.SetActions(minimalActions, UIUserNotificationActionContext.Minimal);
				addOccurrenceCategory.SetActions(actions, UIUserNotificationActionContext.Default);
				return addOccurrenceCategory;
			}
		}

		private UIMutableUserNotificationCategory AfterSnoozeCategory
		{
			get
			{
				var offsets = from i in new Dictionary<int, string>()
								{
									{0,"Just now!"},
									{2,"Skip"},
									{10,"Snooze again" }
								}
							  select new UIMutableUserNotificationAction()
							  {
								  Identifier = string.Format("Add:{0}", i.Key),
								  Title = i.Value,
								  ActivationMode = UIUserNotificationActivationMode.Background,
								  AuthenticationRequired = false,
								  Destructive = i.Key >0,
								  Behavior = UIUserNotificationActionBehavior.Default
							  };

				var afterSnoozeCategory = new UIMutableUserNotificationCategory();
				afterSnoozeCategory.Identifier = "AfterSnooze";
				afterSnoozeCategory.SetActions(offsets.ToArray(), UIUserNotificationActionContext.Default);
				return afterSnoozeCategory;
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

