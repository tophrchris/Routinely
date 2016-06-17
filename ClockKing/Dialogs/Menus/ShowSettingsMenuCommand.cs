using UIKit;
using MonoTouch.Dialog;


namespace ClockKing.Commands
{
	public class ShowSettingsMenuCommand
	{
		protected CheckPointController Controller{ get; set; }
		protected UIBarButtonItem BarButton{ get; set; }

		public ShowSettingsMenuCommand (CheckPointController controller)
		{
			this.Controller = controller;
			var checkPoints = this.Controller.CheckPoints;


			var acs = UIAlertController.Create ("Settings", "settings will live here... kinda?", UIAlertControllerStyle.ActionSheet);

			acs.AddAction (UIAlertAction.Create ("show Notifications", UIAlertActionStyle.Default,
				(a) => this.ShowDialog ()));

			acs.AddAction(UIAlertAction.Create("change sort",UIAlertActionStyle.Default,
				(a)=>
				{
					var app = UIApplication.SharedApplication.Delegate as AppDelegate;
					app.Options.GroupingChoice=
						(app.Options.GroupingChoice==GroupingChoices.ByStatus)?
								GroupingChoices.ByTimeOfDay:
								GroupingChoices.ByStatus;
					
					app.Controller.ConditionallyRefreshData(true);
				}
			));

			acs.AddAction (UIAlertAction.Create ("Toggle Tracing", UIAlertActionStyle.Default,
				(a) => {
					var app = UIApplication.SharedApplication.Delegate as AppDelegate;
					if(app.Options.TracingEnabled)
						controller.notify("Disabling notifications","re-enable using the toggle",iiToastNotification.Unified.ToastNotificationType.Error);
					app.Options.TracingEnabled=!app.Options.TracingEnabled;
					if(app.Options.TracingEnabled)
						controller.notify("Enabling notifications","disable using the toggle",iiToastNotification.Unified.ToastNotificationType.Error);
					

				}
			));

			acs.AddAction(UIAlertAction.Create("month", UIAlertActionStyle.Default,
				(a) => {
					this.Controller.ShowMonthView();	
				//var m = new MonthView();
					//this.Controller.NavigationController.PushViewController(m,true);
			}));

			acs.AddAction(UIAlertAction.Create("Reload Data", UIAlertActionStyle.Default,
			    (a) => this.Controller.ConditionallyRefreshData(true)));

			acs.AddAction(UIAlertAction.Create("reset notifications",UIAlertActionStyle.Destructive,
				(a)=>this.Controller.ResetNotifications()));

			acs.AddAction(UIAlertAction.Create("Trim occurrences",UIAlertActionStyle.Destructive,
				(a)=>checkPoints.RewriteOccurrences()));

			acs.AddAction(UIAlertAction.Create("nevermind",UIAlertActionStyle.Cancel,null));

			this.BarButton = new UIBarButtonItem ("...", UIBarButtonItemStyle.Bordered,(sender, args) =>
				controller.PresentViewController(acs,true,null));
			
		}

		public UIBarButtonItem MenuCommand{get{return this.BarButton;}}

		protected void ShowDialog()
		{

			var root = new RootElement ("Notifications");
			var mtd = new NotificationReviewDialog (this.Controller, root);
			this.Controller.NavigationController.PushViewController (mtd,true);
		}
	}
}

