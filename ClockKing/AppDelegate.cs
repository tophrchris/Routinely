using Foundation;
using UIKit;
using Xamarin.Themes;
using Xamarin.Themes.Core;
using System.Linq;
using System;
using ClockKing.Extensions;
using System.Collections.Generic;
using Xamarin.Themes.TrackBeam;
using ClockKing.Core;

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
		public ClockKingOptions Options { get; set; }
		public DataModel CheckPointData { get; set; }
		public CheckPointController Controller{ get; set; }
		public CommandManager Commands{ get; set; }
		public NotificationManager Notifications{ get; set; }	
		private UIApplicationShortcutItem LastShortcutItem { get; set; }
		public Queue<Action<CheckPointController>> LaunchActions{ get; set; }

		public static ICheckPointDataProvider DefaultDataProvider
		{
			get{
				//var com = new CompositeCheckPointDataProvider ();
				//com.AddProvider (new JSONDataProvider (new PathProvider(".json")));
				//com.AddProvider (new CSVDataProvider (new PathProvider(".csv")));
				return new JSONDataProvider (new PathProvider(".json"));
			}
		}


		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			this.Options = new ClockKingOptions ();
			this.Commands = new CommandManager ();
			this.Notifications = new NotificationManager ();
			this.CheckPointData = new DataModel (DefaultDataProvider);
			this.LaunchActions = new Queue<Action<CheckPointController>> ();


			this.Options.Theme = Themes.TrackBeam;
			this.Options.ApplyTheme();

			this.EnsureIntegrations();
		
			application.SetMinimumBackgroundFetchInterval (UIApplication.BackgroundFetchIntervalMinimum);

			var PerformAdditionalHandling = true;
			if (launchOptions != null) 
			{
				this.LastShortcutItem = launchOptions [UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
				PerformAdditionalHandling = (LastShortcutItem == null);
			}

			return PerformAdditionalHandling;
		}
		public void EnsureIntegrations()
		{
			var application = UIApplication.SharedApplication;
			this.Notifications.EnsureSettings (application);
			this.Notifications.EnsureNotifications(this.CheckPointData);
			ShortcutManager.CreateShortcutItems (application,this.CheckPointData);
			SpotlightManager.PresentGoalsForIndexing(this.CheckPointData.checkPoints);
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

		public override void WillTerminate (UIApplication application)
		{
			//			not sure if i want to do this??
			//			if (this.Controller != null) {
			//				Controller.ResaveCheckpoints ();
			//				Controller.RewriteOccurrences ();
			//			}
		}

		public override void PerformFetch (UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
		{

			ShortcutManager.CreateShortcutItems (application,new DataModel(DefaultDataProvider));
			completionHandler (UIBackgroundFetchResult.NewData);
		}

		//for when the application becomes active(?)
		public override void OnActivated (UIApplication application)
		{
			
			if (this.Controller!=null) 
				this.Controller.ConditionallyRefreshData (true);

			if (LastShortcutItem != null) 
			{
				ShortcutManager.HandleShortcut (application, LastShortcutItem);
				LastShortcutItem = null;
			}
		}

		//for when a user finds a goal via spotlight
		public override bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
		{
			SpotlightManager.HandleUserActivity(application, userActivity);
			return true;
		}

		//for when a user chooses a quick launch shortcut
		public override void PerformActionForShortcutItem (UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			completionHandler (ShortcutManager.HandleShortcut (application, shortcutItem));
		}

		//for when notifications go off while the user is in the app
		public override void ReceivedLocalNotification (UIApplication application, UILocalNotification notification)
		{
			NotificationManager.HandleLocalNotification (application,notification);
		}
			
		//for when the user responds to banner notifications by selecting an action, while using a different app
		public override void HandleAction (UIApplication application, string actionIdentifier, UILocalNotification localNotification, System.Action completionHandler)
		{
			var dataChanged = NotificationManager.HandleNotificationAction (application, localNotification, actionIdentifier);
			if (dataChanged)
				this.RequiresDataRefresh = true;
			
			completionHandler ();
		}
	}
}