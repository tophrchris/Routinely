using System;
using MonoTouch.Dialog;
using UIKit;
using Foundation;



namespace ClockKing
{
	public class Menu:CheckPointDialog
	{
		public static string aboutUrl = @"http://bit.ly/aboutRoutinely";
		public static string feedbackUrl = @"http://bit.ly/RoutinelyFeedback";

		private BooleanElement inactiveSwitch;
		private BooleanElement tracingEnabled;
		private UISegmentedControl groupingChoiceSegments;

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

			this.inactiveSwitch = new BooleanElement("Show Inactive Goals", ClockKingOptions.ShowInactiveGoals);

			this.groupingChoiceSegments = new UISegmentedControl(new CoreGraphics.CGRect(30,0,190,40));

			groupingChoiceSegments.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
			groupingChoiceSegments.TintColor = UIColor.FromRGB(.6f,.3f,.6f);
			groupingChoiceSegments.BackgroundColor = UIColor.FromRGB(.6f,.6f,.6f);
			groupingChoiceSegments.InsertSegment("Status", 0, false);
			groupingChoiceSegments.InsertSegment("Time", 1, false);
			groupingChoiceSegments.InsertSegment("Category", 2, false);
			groupingChoiceSegments.SelectedSegment = (int)ClockKingOptions.GroupingChoice;


			var segHolder = new UIViewElement("Sort:", groupingChoiceSegments, false);
			segHolder.Flags = UIViewElement.CellFlags.DisableSelection;


			var switches = new Section("Goal listing"){ inactiveSwitch,segHolder };

			var debug = new Section("debugging");


			this.tracingEnabled = new BooleanElement("Enabled Trace banners", ClockKingOptions.TracingEnabled);

			debug.Add(tracingEnabled);


			debug.Add(new StringElement("Reload Data", () =>
			{
				Close();
				this.Controller.ConditionallyRefreshData(true);
			}));
			debug.Add(new StringElement("Reset Notifications", () => this.Controller.ResetNotifications()));
			debug.Add(new StringElement("Trim Occurrences", () => this.Controller.CheckPoints.RewriteOccurrences()));


			var support = new Section("Support");
			support.Add(new StringElement("About Routinely", () => NavigateToUrl(aboutUrl)));
			support.Add(new StringElement("Feedback", () => NavigateToUrl(feedbackUrl)));

			this.Root.Add(nav);

			this.Root.Add(switches);
			this.Root.Add(debug);
			this.Root.Add(support);

			inactiveSwitch.ValueChanged += (s, e) =>
			{
				if (ClockKingOptions.ShowInactiveGoals != inactiveSwitch.Value)
				{
					Close();
					inactiveSwitch.Value = ClockKingOptions.ShowInactiveGoals = !ClockKingOptions.ShowInactiveGoals;

					Controller.RespondToModelChanges();
				}
			};

			groupingChoiceSegments.ValueChanged += (sender, e) =>
			{
				if (ClockKingOptions.GroupingChoice != (GroupingChoices)((int)groupingChoiceSegments.SelectedSegment))
				{
					ClockKingOptions.GroupingChoice = (GroupingChoices)((int)groupingChoiceSegments.SelectedSegment);

					Controller.RespondToModelChanges();
				}
			};

			tracingEnabled.ValueChanged += (s, e) =>
			{
				if (ClockKingOptions.TracingEnabled != tracingEnabled.Value)
				{
					if (ClockKingOptions.TracingEnabled)
						Controller.notify("Disabling banners", "re-enable using the toggle", iiToastNotification.Unified.ToastNotificationType.Error);
					ClockKingOptions.TracingEnabled = !ClockKingOptions.TracingEnabled;
					if (ClockKingOptions.TracingEnabled)
						Controller.notify("Enabling banners", "disable using the toggle", iiToastNotification.Unified.ToastNotificationType.Error);


				}

			};


		}

		public void resetToOptions()
		{
			this.tracingEnabled.Value = ClockKingOptions.TracingEnabled;
			this.inactiveSwitch.Value = ClockKingOptions.ShowInactiveGoals;
			this.groupingChoiceSegments.SelectedSegment = (int)ClockKingOptions.GroupingChoice;
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
