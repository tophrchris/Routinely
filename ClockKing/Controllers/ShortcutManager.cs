using System;
using Foundation;
using UIKit;
using ClockKing.Extensions;
using System.Collections.Generic;
using System.Linq;
using Humanizer;


namespace ClockKing
{
	public class ShortcutManager
	{
		static string AddOccurrence = "AddOccurrenceToCheckpoint";
		static string AddCheckpoint = "AddCheckpoint";

		public static void CreateShortcutItems(UIApplication application,DataModel data)
		{


			var Create = new Func< ClockKing.Core.CheckPoint,UIMutableApplicationShortcutItem>(
				(targetCheckPoint)=>{

					var uinfo = new NSDictionary<NSString, NSObject>(new NSString("cp"),new NSString(targetCheckPoint.Name));
					//var humanized = targetCheckPoint.TargetTimeToday.ToUniversalTime().Humanize();

					return  new UIMutableApplicationShortcutItem(AddOccurrence,
						string.Format("{0} {1}",targetCheckPoint.Emoji, targetCheckPoint.Name))
					{
						LocalizedSubtitle=targetCheckPoint.TargetTimeToday.ToString("t"),
						Icon=UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Task),
						UserInfo = uinfo
					};
				});

			var items = new []{ data.LastCheckpoint, data.NextCheckpoint }
				.Where(cp=>cp!=null)
				.Select (cp => Create (cp)).ToList();

			items.Add(new UIMutableApplicationShortcutItem(AddCheckpoint,"Add a new Goal")
				{
					LocalizedSubtitle="Create a new daily goal",
					Icon=UIApplicationShortcutIcon.FromType(UIApplicationShortcutIconType.Add)
					
				});

			application.ShortcutItems =  items.ToArray() ;
		}

		public static bool HandleShortcut(UIApplication application, UIApplicationShortcutItem shortcut)
		{
			var cpKey = new NSString ("cp");
			var app = application.Delegate as AppDelegate;

			var handlers = new Dictionary<string,Action> 
			{ 
				{AddOccurrence,() => 
					{
						var foundCheckpointName = shortcut.UserInfo[cpKey].ToString();
						var foundCheckPoint = app.CheckPointData.checkPoints[foundCheckpointName];
						app.LaunchActions.Enqueue(c=>new AddHistoricOccurrenceCommand().ShowCustomDialog(c,foundCheckPoint));
					}},
				{AddCheckpoint,()=>
					app.LaunchActions.Enqueue(c=>c.AddCommand.ShowDialog())}
				
			};

			handlers [shortcut.Type]();
	
			return true;
		}
	}
}

