using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;
using ClockKing.Extensions;
using EmojiSharp;

namespace ClockKing.Commands
{
	public class ShowSettingsMenuCommand
	{



		protected CheckPointController Controller{ get; set; }
		protected UIBarButtonItem BarButton{ get; set; }

		public ShowSettingsMenuCommand (CheckPointController controller)
		{
			this.Controller = controller;

			var acs = UIAlertController.Create ("Settings", "settings will live here... kinda?", UIAlertControllerStyle.ActionSheet);

			acs.AddAction (UIAlertAction.Create ("show Notifications", UIAlertActionStyle.Default,
				(a) => this.ShowNotificationsDialog ()));

			acs.AddAction(UIAlertAction.Create("change sort",UIAlertActionStyle.Default,
				(a)=>
				{
					var app = UIApplication.SharedApplication.Delegate as AppDelegate;
					app.Options.GroupingChoice=
						(app.Options.GroupingChoice==GroupingChoices.ByStatus)?
								GroupingChoices.ByTimeOfDay:
								GroupingChoices.ByStatus;
					app.RequiresDataRefresh=true;
					app.Controller.ConditionallyRefreshData();
				}
			));

			acs.AddAction(UIAlertAction.Create("reset notifications",UIAlertActionStyle.Destructive,
				(a)=>this.Controller.ResetNotifications()));

			acs.AddAction(UIAlertAction.Create("Trim occurrences",UIAlertActionStyle.Destructive,
				(a)=>this.Controller.RewriteOccurrences()));

			acs.AddAction(UIAlertAction.Create("nevermind",UIAlertActionStyle.Cancel,null));

			this.BarButton = new UIBarButtonItem ("...", UIBarButtonItemStyle.Bordered,(sender, args) =>
				controller.PresentViewController(acs,true,null));
			
		}

		public UIBarButtonItem Button{get{return this.BarButton;}}

		protected void ShowNotificationsDialog()
		{

			var root = new RootElement ("Notifications");
			var mtd = new NotificationReviewDialog (this.Controller, root);
			this.Controller.NavigationController.PushViewController (mtd,true);
		}
	}
}

