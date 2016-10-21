using System;
using MonoTouch.Dialog;
using ClockKing.Core;
using System.Linq;
using UIKit;
using System.Collections.Generic;
using ClockKing.Extensions;
using Humanizer;

namespace ClockKing
{
	public class ScheduledTargetDialog:DialogViewController,iNavigatableDialog
	{
		protected ScheduledTargetTime ScheduledTarget { get; set; }
		protected CheckPoint checkpoint { get; set; }
		protected iCheckpointCommandController Controller { get; set; }
		protected CheckPointDetailDialog dialog { get; set; }

		protected UIDatePicker picker { get; set; }
		protected Section daySelectionSection { get; set; }
		protected Section existingTargetsSection { get; set; }
		protected BooleanElement inactiveSwitch { get; set; }

		public ScheduledTargetDialog (RootElement root, ScheduledTargetTime target,CheckPoint checkpoint, iCheckpointCommandController Controller,CheckPointDetailDialog dialog):base(root,true)
		{
			this.ScheduledTarget = target;
			this.checkpoint = checkpoint;
			this.Controller = Controller;
			this.dialog = dialog;

			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.Time };
			this.inactiveSwitch = new BooleanElement("Goal is inactive on these days?", !target.TargetTime.HasValue);

			var pickerElement = new UIViewElement (string.Empty, this.picker, false);

			this.daySelectionSection = new Section ("Effective Days");
			this.existingTargetsSection = new Section ("Existing Alternative Targets","Tap to remove alternative target from specified days.");
			var timeSpecificationSection = new Section("Alternative Target:",
			    @"Specify a different target time for the goal, which will be effective on the days selected below.");



			inactiveSwitch.ValueChanged += (so, ev) => {
				if (inactiveSwitch.Value) 
					timeSpecificationSection.Remove (pickerElement);
				 else 
					timeSpecificationSection.Insert (1, pickerElement);
			};


			timeSpecificationSection.Add(inactiveSwitch);
			if (ScheduledTarget.TargetTime.HasValue)
				timeSpecificationSection.Add (pickerElement);


			Root.Add(new CheckPointCellSection(checkpoint));

			Root.Add(new Section[] { timeSpecificationSection, daySelectionSection });

			Root.Add (new Section ("","Tap to delete this Alternative Target.") {new StringElement ("Remove",
				() => this.RemoveScheduledTarget())
				{Alignment=UITextAlignment.Center
				 }});



			this.NavigationItem.SetRightBarButtonItem (
				new UIBarButtonItem (UIBarButtonSystemItem.Save,
					(so, ev) => this.Save()), true);

			this.NavigationItem.SetLeftBarButtonItem (
				new UIBarButtonItem (UIBarButtonSystemItem.Cancel,
					(so, ev) =>this.Close() ), true);

			this.Render ();

		}

		protected void Render()
		{
			this.picker.Date = (DateTime.Today + (ScheduledTarget.TargetTime ?? DateTime.Now.TimeOfDay)).ToUniversalTime().ToNSDate ();
			this.inactiveSwitch.Value = !ScheduledTarget.TargetTime.HasValue;
			daySelectionSection.Clear ();
			existingTargetsSection.Clear ();
			Root.Remove (existingTargetsSection);

			var daysAlreadyScheduled = new Dictionary<DayOfWeek,TimeSpan?> ();

			foreach (var t in checkpoint.ScheduledTargets.Except(new []{ScheduledTarget}))
				foreach (var dy in t.ApplicableDays)
					if(!daysAlreadyScheduled.ContainsKey(dy))
						daysAlreadyScheduled.Add (dy, t.TargetTime);

			for (var i = 0; i <= 6; i++)
				if(!daysAlreadyScheduled.ContainsKey((DayOfWeek)i))
					daySelectionSection.Add (new CheckboxElement (((DayOfWeek)i).ToString (),
						ScheduledTarget.ApplicableDays.Contains (((DayOfWeek)i))));

			if (daysAlreadyScheduled.Any ()) 
			{
				Root.Insert (3, existingTargetsSection);
				existingTargetsSection.AddAll (
					daysAlreadyScheduled
					.OrderBy(kv=>kv.Key)
					.Select (kv => new StringElement (kv.Key.ToString (),
						()=>this.FreeUpDayOfWeek(kv.Key))
						{
						Value=kv.Value.HasValue ?kv.Value.Value.ToAMPMString () : "Inactive"
						}));	
			}
			else 
			{
				Root.Remove (existingTargetsSection);
			}
				
		}
		protected void FreeUpDayOfWeek(DayOfWeek toClear)
		{
			var handler = new Action<UIAlertAction> ((action) => {
				var found = checkpoint.ScheduledTargets.First (st => st.ApplicableDays.Contains (toClear));
				if (found != null) {
					found.ApplicableDays = found.ApplicableDays.Except (new[]{ toClear }).ToArray ();
				}
				Render ();
			});

			var controller = SharedDialogs.ConfirmationDialog (handler,
				Message:"Would you like to remove the existing Scheduled Target for {0}?".FormatWith(toClear));
			this.PresentModalViewController (controller, true);
		}

		protected void RemoveScheduledTarget()
		{
			var handler = new Action<UIAlertAction> ((action) => {
				checkpoint.RemoveScheduledTarget (ScheduledTarget);
				this.Close (true);
			});
			var controller = SharedDialogs.ConfirmationDialog (handler,
				Message:"This will delete this Alternative Target.",
				yes:"OK");
			this.PresentModalViewController(controller,true);

		}

		protected void Save()
		{
			if (inactiveSwitch.Value)
				ScheduledTarget.TargetTime = null;
			else
				ScheduledTarget.TargetTime = picker.Date.ToDateTime().ToLocalTime().TimeOfDay;

			ScheduledTarget.ApplicableDays =
				daySelectionSection.Elements
					.Where (i => ((CheckboxElement)i).Value)
					.Select (i => (DayOfWeek)Enum.Parse(typeof(DayOfWeek),i.Caption))
					.ToArray ();

			var otherExisting = 
				checkpoint.ScheduledTargets.Except (new[]{ ScheduledTarget })
					.FirstOrDefault (st => 
						{
							if(!st.TargetTime.HasValue && !ScheduledTarget.TargetTime.HasValue)
								return true;
							if(st.TargetTime.HasValue && ScheduledTarget.TargetTime.HasValue)
								if((int)st.TargetTime.Value.TotalMinutes==(int)ScheduledTarget.TargetTime.Value.TotalMinutes)
									return true;
							return false;
						});
			
			if (otherExisting != null) {
				otherExisting.ApplicableDays = otherExisting.ApplicableDays.Concat (ScheduledTarget.ApplicableDays).Distinct().ToArray();
				checkpoint.RemoveScheduledTarget (ScheduledTarget);
			}
			this.Close (true);
		}

		protected void Close(bool saveFirst=false)
		{
			if (saveFirst)
			{
				
				Controller.ResaveCheckpoints ();
				if(dialog!=null)
					dialog.RespondToChanges ();
			}
			ResetNavigation ();
		}

		#region iNavigatableDialog implementation

		public void ResetNavigation (bool refreshData=false)
		{
			this.DeactivateController (true);
		}

		#endregion
	}
}

