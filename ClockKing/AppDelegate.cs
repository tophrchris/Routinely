﻿using Foundation;
using UIKit;
using Xamarin.Themes;
using Xamarin.Themes.Core;
using System.Linq;
using System;
using ClockKing.Extensions;
using System.Collections.Generic;
using Xamarin.Themes.TrackBeam;
using ClockKing.Core;
using Google.Analytics;
using System.Diagnostics;
using MTiRate;

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

		static AppDelegate()
		{
			RatingsManager.ConfigureRatingsPrompt();
		}
			
		public bool RequiresDataRefresh { get; set; }
		public DataModel CheckPointData { get; set; }
		public CheckPointController Controller{ get; set; }
		public CommandManager Commands{ get; set; }
		public NotificationManager Notifications{ get; set; }	
		private UIApplicationShortcutItem LastShortcutItem { get; set; }
		public Queue<Action<CheckPointController>> LaunchActions{ get; set; }
		public SidebarNavigation.SidebarController Sidebar;
		private NSObject observer;
		private Google.Analytics.ITracker tracking { get; set; }

		public static ICheckPointDataProvider DefaultDataProvider
		{
			get{
				var com = new CompositeCheckPointDataProvider ();
				com.AddProvider (new JSONDataProvider (new PathProvider(".json")));
				com.AddProvider(new JSONDataProvider(new AppGroupPathProvider(".json")));
				return com;
			}
		}

		public void Track(string screenName)
		{
			EnsureTracking();
			this.tracking.Set(GaiConstants.ScreenName, screenName);
			Debug.WriteLine("on screen:" + screenName);
			this.tracking.Send(DictionaryBuilder.CreateScreenView().Build());
		}

		public void Track(string category, string action, string label, int value=0)
		{
			EnsureTracking();
			Debug.WriteLine(string.Format("{0}:{1}:{2}", category, action, label));
			this.tracking.Send(DictionaryBuilder.CreateEvent(category, action, label, value).Build());
		}

		private void EnsureTracking()
		{
			if (this.tracking == null)
			{
				Gai.SharedInstance.DispatchInterval = 20;
				Gai.SharedInstance.TrackUncaughtExceptions = true;
				this.tracking = Gai.SharedInstance.GetTracker("UA-82950326-1");
			}
		}


		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			LoadOptions();

			this.Commands = new CommandManager();
			this.Notifications = new NotificationManager();
			this.CheckPointData = new DataModel(DefaultDataProvider);
			this.LaunchActions = new Queue<Action<CheckPointController>>();


			this.EnsureIntegrations();
			this.EnsureTracking();

			application.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);

			var PerformAdditionalHandling = true;
			if (launchOptions != null)
			{
				this.LastShortcutItem = launchOptions[UIApplication.LaunchOptionsShortcutItemKey] as UIApplicationShortcutItem;
				PerformAdditionalHandling = (LastShortcutItem == null);
			}

			Window = new UIWindow(UIScreen.MainScreen.Bounds);

			// set our root view controller with the sidebar menu as the apps root view controller
			var root = new RootViewController();
			this.Sidebar = root.SideBar;
			Window.RootViewController = root;

			Window.MakeKeyAndVisible();



			return PerformAdditionalHandling;
		}

		private DateTime lastReload { get; set; }
		void LoadOptions()
		{
			ClockKingOptions.LoadDefaultValues();

			this.observer = NSNotificationCenter.DefaultCenter.AddObserver((NSString)"NSUserDefaultsDidChangeNotification", (NSNotification obj) =>
			{
				if ((DateTime.Now - lastReload).TotalSeconds > 1) 
				{
					if (obj.UserInfo != null)
						Debug.WriteLine ("notification:" + obj.UserInfo.ToString ());
					else
						Debug.WriteLine ("notification: no user info");

					if (this.Controller != null) {
						if (this.Window != null) {
							InvokeOnMainThread(() =>
							{
								this.Controller.RespondToModelChanges();
								var menu = ((RootViewController)this.Window.RootViewController).OptionsMenu;
								menu.resetToOptions();
							});
						}
						this.Commands.ConstructCommands ();
					}
					lastReload = DateTime.Now;
				}
			});

			ClockKingOptions.ApplyTheme();
		}

		public void EnsureIntegrations()
		{
			var application = UIApplication.SharedApplication;
			this.Notifications.EnsureSettings ();
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
			Debug.WriteLine ("did enter background");
			this.Controller.Refresher.StopRefresher ();
		}

		public override void WillEnterForeground (UIApplication application)
		{
			// Called as part of the transiton from background to active state.
			// Here you can undo many of the changes made on entering the background.
			Debug.WriteLine ("will enter foreground");
			this.Controller.Refresher.Restart ();
		}

		public override void WillTerminate (UIApplication application)
		{
			this.Controller.Refresher.StopRefresher ();
			if (observer != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
				observer = null;
			}
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

		public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
		{
			LaunchActions.Enqueue((c) => 
			{
				var guid = Guid.Parse( url.Host);
				var f = this.CheckPointData.checkPoints.Values.First((g) =>g.UniqueIdentifier==guid );

				this.Track("Widget", f.IsMissed ? "ViewMissed" : "ViewNext", f.Name);

				c.ShowDetailDialogFor(f) ;
			} );
			return true;
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