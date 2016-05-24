using Foundation;
using UIKit;
using Xamarin.Themes;
using Xamarin.Themes.Core;
using System.Linq;
using System;
using ClockKing.Extensions;

namespace ClockKing
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations

		public override UIWindow Window {
			get;
			set;
		}
			
		public bool RequiresDataRefresh { get; set; }

		public CheckPointController Controller{ get; set; }
		public CommandManager Commands{ get; set; }
		public NotificationManager Notifications{ get; set; }	

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			//CashflowTheme.Apply ();
			//ThemeManager.Register<TrackBeamTheme> ().Apply ();
			FitpulseTheme.Apply ();
 
			this.Commands = new CommandManager ();
			this.Notifications = new NotificationManager ();
			this.Notifications.EnsureSettings (application);
			this.RequiresDataRefresh = true;
			// Override point for customization after application launch.
			// If not required for your application you can safely delete this method
			return true;
		}

		public override void OnResignActivation (UIApplication application)
		{
			// Invoked when the application is about to move from active to inactive state.
			// This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
			// or when the user quits the application and it begins the transition to the background state.
			// Games should use this method to pause the game.
		}

		public override void DidEnterBackground (UIApplication application)
		{
			// Use this method to release shared resources, save user data, invalidate timers and store the application state.
			// If your application supports background exection this method is called instead of WillTerminate when the user quits.
		}

		public override void WillEnterForeground (UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
		}

		public override void OnActivated (UIApplication application)
		{
			if (this.Controller!=null) 
				this.Controller.ConditionallyRefreshData ();

			// Restart any tasks that were paused (or not yet started) while the application was inactive. 
			// If the application was previously in the background, optionally refresh the user interface.
		}

		public override void WillTerminate (UIApplication application)
		{
			// Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
		}

		public override void ReceivedLocalNotification (UIApplication application, UILocalNotification notification)
		{
			try{
				
				var opts = UIAlertController.Create(notification.AlertTitle,notification.AlertBody,UIAlertControllerStyle.ActionSheet);

				opts.AddAction(UIAlertAction.Create("Done!",UIAlertActionStyle.Default,
					(a)=>this.Controller.AddOccurrenceToCheckPoint(notification.AlertTitle,0)));

				opts.AddAction(UIAlertAction.Create("Snooze",UIAlertActionStyle.Default,
					(a)=>{
						notification.FireDate=DateTime.Now.AddMinutes(10).ToUniversalTime().ToNSDate();
						application.ScheduleLocalNotification(notification);
					}));

				opts.AddAction(UIAlertAction.Create("Cancel",UIAlertActionStyle.Cancel,null));

				this.Window.RootViewController.PresentViewController (opts, true, null);
			}catch{
			}

		}

		/// <summary>
		/// TODO: move this implementation *somewhere*;  either commands or controller?
		/// </summary>
		public override void HandleAction (UIApplication application, string actionIdentifier, UILocalNotification localNotification, System.Action completionHandler)
		{
			var data = new DataModel (false);
			var found = data.CheckPointPairs.Select (cpp => cpp.firstEvent).FirstOrDefault (c => c.Name == localNotification.AlertTitle);
			var actionBits = actionIdentifier.Split(':');
			var mins = int.Parse (actionBits [1]);	
			if (mins > 0) {
				localNotification.FireDate = DateTime.Now.AddMinutes (mins).ToUniversalTime ().ToNSDate ();
				application.ScheduleLocalNotification (localNotification);

			} else {
				var occ = found.CreateOccurrence (DateTime.Now.AddMinutes (mins));
				data.SaveOccurrence (occ);
				this.RequiresDataRefresh = true;
			}
			completionHandler ();

		}
	}
}


