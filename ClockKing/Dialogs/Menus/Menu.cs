using System;
using MonoTouch.Dialog;
using UIKit;


namespace ClockKing
{
	public class Menu:CheckPointDialog
	{
		public Menu() :base()
		{
			this.Root = new RootElement("Options");

			var notifications = new NotificationReviewDialog();
			var monthView = new MonthView();

			var nav = new Section("Navigation");
			nav.Add(new StringElement("Month",() => ShowDialog(monthView)));
			nav.Add(new StringElement("Notifications", () => ShowDialog(notifications)));


			var switches = new Section("Switches");
			switches.Add(new StringElement("Toggle Sort",
			() => {
				Close();
				App.Options.GroupingChoice =
					(App.Options.GroupingChoice == GroupingChoices.ByStatus) ?
						GroupingChoices.ByTimeOfDay :
						GroupingChoices.ByStatus;
				Controller.RespondToModelChanges();
			}));
			switches.Add(new StringElement("Toggle Tracing",
			() => {
			if (App.Options.TracingEnabled)
					Controller.notify("Disabling notifications", "re-enable using the toggle", iiToastNotification.Unified.ToastNotificationType.Error);
				App.Options.TracingEnabled = !App.Options.TracingEnabled;
				if (App.Options.TracingEnabled)
					Controller.notify("Enabling notifications", "disable using the toggle", iiToastNotification.Unified.ToastNotificationType.Error);
			}));

			var debug = new Section("debugging");
			debug.Add(new StringElement("Reload Data", () =>
			{
				Close();
				this.Controller.ConditionallyRefreshData(true);
			}));
			debug.Add(new StringElement("Reset Notifications", () => this.Controller.ResetNotifications()));
			debug.Add(new StringElement("Trim Occurrences", () => this.Controller.CheckPoints.RewriteOccurrences()));

			this.Root.Add(nav);
			this.Root.Add(switches);
			this.Root.Add(debug);
		}

		public void ShowDialog(UIViewController dialog)
		{
			this.ContentNavigation.PushViewController(dialog, true);
			Close();
		}
		public void Close()
		{
			this.Sidebar.ToggleMenu();
		}
	}
}


/*


			acs.AddAction(UIAlertAction.Create("Reload Data", UIAlertActionStyle.Default,
			    (a) => this.Controller.ConditionallyRefreshData(true)));

			acs.AddAction(UIAlertAction.Create("reset notifications",UIAlertActionStyle.Destructive,
				(a)=>this.Controller.ResetNotifications()));

			acs.AddAction(UIAlertAction.Create("Trim occurrences",UIAlertActionStyle.Destructive,
				(a)=>checkPoints.RewriteOccurrences()));

			acs.AddAction(UIAlertAction.Create("nevermind",UIAlertActionStyle.Cancel,null));
*/