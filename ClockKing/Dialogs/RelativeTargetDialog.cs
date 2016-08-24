using System;
using ClockKing.Core;
using MonoTouch.Dialog;
using UIKit;
using System.Linq;
using ClockKing.Extensions;
using System.Collections.Generic;

namespace ClockKing
{
	public class RelativeTargetDialog:CheckPointDialog
	{
		private CheckPoint editing { get; set; }
		private RadioGroup goalGroup { get; set; } = new RadioGroup(0);
		private UIDatePicker offsetPicker { get; set; }
		private UIViewElement offsetElement { get; set; }
		private List<CheckPoint> potentialTargetGoals { get; set; }


		public RelativeTargetDialog(CheckPoint toEdit):base()
		{
			this.editing = toEdit;
			this.Root = new RootElement("New Relative Target");
			this.Style = UITableViewStyle.Grouped;

			var groupingRoot = new RootElement("Goals:", goalGroup);

			var hint = string.Format("the target time of {0} will be relative to the goal you select.", editing.Name);

			var groupingSection = new Section("Select a goal:",hint);
			groupingRoot.Add(groupingSection);

			this.potentialTargetGoals = this.App.CheckPointData.checkPoints.Values
				.Where(cp => cp.UniqueIdentifier != editing.UniqueIdentifier)
				.Where(cp => (cp.RelativeTarget == null) ? true : (cp.RelativeTarget.RelatedCheckPointGuid != editing.UniqueIdentifier))
				.OrderBy(cp=>cp.TargetTimeToday)
				.ToList();

			foreach (var g in this.potentialTargetGoals)
			{
				var e = new RadioElement(string.Format("{0}{1}, at  {2}",g.Emoji,g.Name, g.TargetTimeToday.ToString("t")));
				e.Tapped += () => this.RespondToSelection();
				groupingSection.Add(e);
			}
			var goalSection = new Section("Select a goal:",hint) { groupingRoot };

			offsetPicker =  new UIDatePicker() { Mode = UIDatePickerMode.CountDownTimer };
			offsetPicker.Date = DateTime.Today.Date.ToUniversalTime().ToNSDate();
			offsetPicker.MinuteInterval = 15;


			this.offsetElement = new UIViewElement(string.Empty,offsetPicker,false);


			var offsetSection = new Section("Relative time:") { this.offsetElement };

			this.Root.Add(goalSection);
			this.Root.Add(offsetSection);

			this.NavigationItem.SetRightBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Save,
				                    (so, ev) => this.Save()), true);

			this.NavigationItem.SetLeftBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Cancel,
				                    (so, ev) => this.Close()), true);
		}

		void RespondToSelection()
		{
			this.NavigationController.PopViewController(true);
		}

		void Save()
		{
			var selectedIndex = goalGroup.Selected;
			var found = this.potentialTargetGoals.ElementAt(selectedIndex);

			bool consistent = EnsureGraphConsistency(this.editing,found);

			if (!consistent)
			{
				var d = SharedDialogs.ErrorDialog("Oops",string.Format( "the goal you've chosen as your related goal is already related to this {0}! please choose a different goal.",editing.Name));
				this.Controller.PresentViewController(d,true,null);
				return;
			}

			var n  = new RelativeTargetTime();
			n.RelatedCheckPoint = found;
			n.Offset = offsetPicker.Date.ToDateTime().ToLocalTime().TimeOfDay;

			this.editing.RelativeTarget = n;
			//TODO:lets get a reference to a manager here...
			this.App.CheckPointData.SaveCheckPoints();
			this.Close();
		}

		bool EnsureGraphConsistency(CheckPoint test, CheckPoint proposed)
		{
			if (proposed.RelativeTarget!=null &&
			    proposed.RelativeTarget.RelatedCheckPoint == test)
				return false;
			
			if (proposed.RelativeTarget == null)
				return true;
			
			return EnsureGraphConsistency(test, proposed.RelativeTarget.RelatedCheckPoint);
		}

		void Close()
		{
			this.DeactivateController(true);
		}
	}
}

