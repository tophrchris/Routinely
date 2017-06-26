using System;
using MonoTouch.Dialog;
using UIKit;
using Foundation;



namespace ClockKing
{
	public class Menu : CheckPointDialog
	{
		public static string aboutUrl = @"http://bit.ly/RoutinelyDevBlog";
		public static string feedbackUrl = @"http://bit.ly/RoutinelyFeedback";

		private BooleanElement inactiveSwitch;
		private BooleanElement tracingEnabled;
		private Section debugSection;
		private RadioGroup GroupingOptions {get;set;}= new RadioGroup(0);

		public Menu() : base()
		{
			this.Pushing = true;

			this.Root = new RootElement("Options");
			this.Root.UnevenRows = true;

			var notifications = new NotificationReviewDialog();

			var nav = new Section("Navigation");
			nav.Add(new StringElement(EmojiSharp.Emoji.CALENDAR.Unified+  " View History", () =>ShowDialog(new MonthView())));
			nav.Add(new StringElement("Add New Goal", () => buildAndShowAddDialog()));


			this.inactiveSwitch = new BooleanElement("Show Inactive Goals", ClockKingOptions.ShowInactiveGoals);
			this.GroupingOptions.Selected = (int)ClockKingOptions.GroupingChoice;

			var groupingRoot = new RootElement("Goals grouped by:", GroupingOptions);
			var groupingSection = new Section("Grouping Options");
			groupingRoot.Add(groupingSection);
			foreach (var s in new[] { "Status", "Time of Day", "Category" })
			{
				var groupOption = new RadioElement(s);
				groupOption.Tapped += () => OnGroupingChoiceChanged();
				groupingSection.Add(groupOption);
			}


			var switches = new Section("Goal listing"){ inactiveSwitch,groupingRoot };

			this.debugSection = new Section("debugging");


			this.tracingEnabled = new BooleanElement("Enabled Trace banners", ClockKingOptions.TracingEnabled);

			debugSection.Add(tracingEnabled);


			debugSection.Add(new StringElement("Reload Data", () =>
			{
				Close();
				this.Controller.ConditionallyRefreshData(true);
			}));
			debugSection.Add(new StringElement("Show Pending Notifications", () => ShowDialog(notifications)));
			debugSection.Add(new StringElement("Reset Notifications", () => this.Controller.ResetNotifications()));
			debugSection.Add(new StringElement("Trim Occurrences", () => this.Controller.CheckPoints.RewriteOccurrences()));


			var support = new Section("Support");
			support.Add(new StringElement("About Routinely", () => NavigateToUrl(aboutUrl)));
			support.Add(new StringElement("Feedback", () => NavigateToUrl(feedbackUrl)));


			var filePath = NSBundle.MainBundle.PathForResource("Info", "plist");
			var dict = NSDictionary.FromFile(filePath);
			var BuildVer = dict["CFBundleVersion"].ToString();
			var appVer = dict["CFBundleShortVersionString"].ToString();
			support.Add(new StringElement("Version", string.Format("{0} ({1})",appVer,BuildVer)));
			
			this.Root.Add(nav);
			this.Root.Add(switches);
			this.Root.Add(support);
			if(ClockKingOptions.EnableDebugOptions)
				this.Root.Add(debugSection);


			inactiveSwitch.ValueChanged += (s, e) =>
			{
				if (ClockKingOptions.ShowInactiveGoals != inactiveSwitch.Value)
				{
					Close();
					inactiveSwitch.Value = ClockKingOptions.ShowInactiveGoals = !ClockKingOptions.ShowInactiveGoals;

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
			bool redrawRequired = false;
			this.tracingEnabled.Value = ClockKingOptions.TracingEnabled;
			this.inactiveSwitch.Value = ClockKingOptions.ShowInactiveGoals;
			this.GroupingOptions.Selected = (int) ClockKingOptions.GroupingChoice;
			if (OnGroupingChoiceChanged())
				redrawRequired = true;
			if (ClockKingOptions.EnableDebugOptions)
			{
				if (debugSection.Parent == null)
				{
					this.Root.Add(debugSection);
					System.Diagnostics.Debug.WriteLine("section added");
					redrawRequired = true;

				}
			}
			else {
				if (debugSection.Parent != null)
				{
					this.Root.Remove(debugSection);
					System.Diagnostics.Debug.WriteLine("section removed");
					redrawRequired = true;
				}
			}
			if (redrawRequired)
				this.ReloadData();
		}

		private bool OnGroupingChoiceChanged()
		{
			var didChange = false;
			GroupingChoices selected = (GroupingChoices)GroupingOptions.Selected;
			if (selected != ClockKingOptions.GroupingChoice)
			{
				didChange = true;
				ClockKingOptions.GroupingChoice = selected;
				this.Controller.RespondToModelChanges();
			}
			return didChange;
		}

		public void NavigateToUrl(string url)
		{
			var target = new NSUrl(url);

			UIApplication.SharedApplication.OpenUrl(target);
		}

		public void ShowDialog(UIViewController dialog,bool autoClose=true)
		{
			try
			{
				this.ContentNavigation.PushViewController(dialog, true);
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
			finally
			{
				if (autoClose)
					this.Close();
			}
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
