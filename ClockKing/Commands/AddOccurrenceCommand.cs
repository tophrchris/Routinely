using System;
using ClockKing.Core;
using UIKit;
using System.Linq;

namespace ClockKing
{
	public class AddOccurrenceCommand:EnabledCheckpointCommand
	{
		public AddOccurrenceCommand(UIColor color,string label):base(color,label){}
		public AddOccurrenceCommand():base(UIColor.Green,"Add")
		{
			this.ChangesCheckpoint = false;
			this.Category = "Right";
			this.LongName = "Add an occurrence right now";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{

				var o = AddOccurrenceToCheckpoint (controller, checkPoint);
				var added = o != null;
				return added;

		}

		protected Occurrence AddOccurrenceToCheckpoint(CheckPointController controller, CheckPoint checkPoint,int mins=0)
		{
			Occurrence created=null;
			if (checkPoint.CompletedToday) {
				var c = SharedDialogs.ConfirmationDialog (
					(a) => created = controller.AddOccurrenceToCheckPoint (checkPoint, mins),
					"You've already completed this goal today!",
					"would you like to add another occurrence?",
					yes:"Yes, add another occurrence.",
					YesIsDestructive:false);
				c.AddAction (UIAlertAction.Create ("No, replace existing..", UIAlertActionStyle.Destructive,
					(m) => {
						var remove = checkPoint.Occurrences.Where(o=>o.Date==DateTime.Today).ToList();
						remove.ForEach(r=>checkPoint.RemoveOccurrence(r));
						created = controller.AddOccurrenceToCheckPoint(checkPoint,mins);
					}));
				controller.PresentModalViewController (c, true);
			} else {
			 	created = controller.AddOccurrenceToCheckPoint (checkPoint, mins);
			}
			return created;
		}
	}
}

