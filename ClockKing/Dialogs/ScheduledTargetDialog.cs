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
	public class ScheduledTargetDialog:DialogViewController
	{
		protected ScheduledTargetTime ScheduledTarget { get; set; }
		protected CheckPoint checkpoint { get; set; }
		protected CheckPointController Controller { get; set; }
		protected CheckPointDetailDialog dialog { get; set; }

		protected UIDatePicker picker { get; set; }
		protected Section days { get; set; }
		protected Section existingTargets { get; set; }
		protected BooleanElement inactiveSwitch { get; set; }

		public ScheduledTargetDialog (RootElement root, ScheduledTargetTime target,CheckPoint checkpoint, CheckPointController Controller,CheckPointDetailDialog dialog):base(root,true)
		{
			this.ScheduledTarget = target;
			this.checkpoint = checkpoint;
			this.Controller = Controller;
			this.dialog = dialog;

			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.Time };
			var pickerElement = new UIViewElement (string.Empty, this.picker, false);

			this.inactiveSwitch = new BooleanElement ("Goal is inactive on these days?",!target.TargetTime.HasValue);
			this.days = new Section ("days");
			this.existingTargets = new Section ("Existing scheduled targets");
			
			Root.Add (new Section ("Goal"){ new CheckPointElement (checkpoint, Controller) });

			var timeSection = new Section ("Scheduled Target"){inactiveSwitch};

			Root.Add (new Section[]{ timeSection, days });

			inactiveSwitch.ValueChanged += (so, ev) => {
				if (inactiveSwitch.Value) 
					timeSection.Remove (pickerElement);
				 else 
					timeSection.Insert (1, pickerElement);
			};

			if (ScheduledTarget.TargetTime.HasValue)
				timeSection.Add (pickerElement);

			Root.Add (new Section ("Delete") {new StringElement ("delete",
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
			days.Clear ();
			existingTargets.Clear ();
			Root.Remove (existingTargets);

			var daysAlreadyScheduled = new Dictionary<DayOfWeek,TimeSpan?> ();

			foreach (var t in checkpoint.ScheduledTargets.Except(new []{ScheduledTarget}))
				foreach (var dy in t.ApplicableDays)
					if(!daysAlreadyScheduled.ContainsKey(dy))
						daysAlreadyScheduled.Add (dy, t.TargetTime);

			for (var i = 0; i <= 6; i++)
				if(!daysAlreadyScheduled.ContainsKey((DayOfWeek)i))
					days.Add (new CheckboxElement (((DayOfWeek)i).ToString (),
						ScheduledTarget.ApplicableDays.Contains (((DayOfWeek)i))));

			if (daysAlreadyScheduled.Any ()) 
			{
				Root.Insert (3, existingTargets);
				existingTargets.AddAll (
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
				Root.Remove (existingTargets);
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
				Message:"Would you like to remove the existing scheduled target for {0}?".FormatWith(toClear));
			this.PresentModalViewController (controller, true);
		}

		protected void RemoveScheduledTarget()
		{
			var handler = new Action<UIAlertAction> ((action) => {
				checkpoint.RemoveScheduledTarget (ScheduledTarget);
				Controller.ResaveCheckpoints ();
				dialog.RespondToChanges ();
				this.Close ();
			});
			var controller = SharedDialogs.ConfirmationDialog (handler,
				Message:"This will delete the scheduled target",
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
				days.Elements
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
				
			Controller.ResaveCheckpoints ();
			dialog.RespondToChanges ();
			this.Close ();
		}

		protected void Close()
		{
			this.DeactivateController (true);
		}
	}
}

