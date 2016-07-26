using System;
using MonoTouch.Dialog;
using UIKit;
using Foundation;



namespace ClockKing
{
	public class Menu:CheckPointDialog
	{
		public static string aboutUrl = @"http://bit.ly/aboutRoutinely";

		public Menu() : base()
		{
			this.Pushing = true;

			this.Root = new RootElement("Options");
			this.Root.UnevenRows = true;

			var notifications = new NotificationReviewDialog();
			var monthView = new MonthView();

			var nav = new Section("Navigation");
			nav.Add(new StringElement(EmojiSharp.Emoji.CALENDAR.Unified+  " View History", () => ShowDialog(monthView)));
			nav.Add(new StringElement("Add New Goal", () => buildAndShowAddDialog()));
			nav.Add(new StringElement("Show Pending Notifications", () => ShowDialog(notifications)));

			var inactiveSwitch = new BooleanElement("Show Inactive Goals", true);

			inactiveSwitch.ValueChanged += (s, e) =>{
				Close();
				inactiveSwitch.Value= App.Options.ShowInactiveGoals = !App.Options.ShowInactiveGoals;
				Controller.RespondToModelChanges();
			};

			var seg = new UISegmentedControl(new CoreGraphics.CGRect(30,0,190,40));

			seg.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
			seg.TintColor = UIColor.FromRGB(.6f,.3f,.6f);
			seg.BackgroundColor = UIColor.FromRGB(.6f,.6f,.6f);
			seg.InsertSegment("Status", 0, false);
			seg.InsertSegment("Time", 1, false);
			seg.InsertSegment("Category", 2, false);
			seg.SelectedSegment = (int)App.Options.GroupingChoice;
			seg.ValueChanged += (sender, e) =>
			{
				App.Options.GroupingChoice = (GroupingChoices)((int)seg.SelectedSegment);
				App.Controller.RespondToModelChanges();
			};
			var segHolder = new UIViewElement("Sort:", seg, false);
			segHolder.Flags = UIViewElement.CellFlags.DisableSelection;




			var switches = new Section("Goal listing"){ inactiveSwitch,segHolder };

			var debug = new Section("debugging");


			var te = new BooleanElement("Enabled Trace banners", false);
			te.ValueChanged += (s, e) =>
			{
				if (App.Options.TracingEnabled)
					Controller.notify("Disabling banners", "re-enable using the toggle", iiToastNotification.Unified.ToastNotificationType.Error);
				App.Options.TracingEnabled = !App.Options.TracingEnabled;
				if (App.Options.TracingEnabled)
					Controller.notify("Enabling banners", "disable using the toggle", iiToastNotification.Unified.ToastNotificationType.Error);
			};
			debug.Add(te);


			debug.Add(new StringElement("Reload Data", () =>
			{
				Close();
				this.Controller.ConditionallyRefreshData(true);
			}));
			debug.Add(new StringElement("Reset Notifications", () => this.Controller.ResetNotifications()));
			debug.Add(new StringElement("Trim Occurrences", () => this.Controller.CheckPoints.RewriteOccurrences()));


			var support = new Section("Support");
			support.Add(new StringElement("About Routinely", () => NavigateToUrl(aboutUrl)));


			this.Root.Add(nav);

			this.Root.Add(switches);
			this.Root.Add(debug);
			this.Root.Add(support);


		}

		public void NavigateToUrl(string url)
		{
			var target = new NSUrl(url);

			UIApplication.SharedApplication.OpenUrl(target);
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
		public void buildAndShowAddDialog()
		{
			var mtd = new CheckPointEditingDialog(this.Controller.CheckPoints, new RootElement("Add..."), true);
			ShowDialog(mtd);
		}
	}
}
