using System;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;

namespace ClockKing
{
	public class AddOccurrenceCommand:EnabledCheckpointCommand
	{
		public AddOccurrenceCommand(string color,string label):base(color,label){}
		public AddOccurrenceCommand():base("Green","Add")
		{
			this.ChangesCheckpoint = false;
			this.Category = "Right";
			this.LongName = "Add an occurrence right now";
		}

		public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
		{

				var o = AddOccurrenceToCheckpoint (controller, checkPoint);
				var added = o != null;
				return added;

		}

        protected Occurrence AddOccurrenceToCheckpoint(iCheckpointCommandController checkPoints, CheckPoint checkPoint,int mins=0)
		{
			Occurrence created=null;
			if (checkPoint.CompletedToday) {

                var ok = new ModalChoice(){Label="Yes, add another occurrence.",
                    Handler=()=>checkPoints.AddOccurrenceToCheckPoint(checkPoint,mins) };
                var no = new ModalChoice(){Label="No, Replace existing.",
                    Handler=()=>
                        {
                            var remove = checkPoint.Occurrences.Where(o=>o.Date==DateTime.Today).ToList();
                            foreach(var r in remove)
                                checkPoint.RemoveOccurrence(r);
                            if (remove.Any ())
                                checkPoints.RewriteOccurrences();
                            created = checkPoints.AddOccurrenceToCheckPoint(checkPoint,mins);
                        }};
                var cancel = new ModalChoice(){ Label = "Nevermind", Cancel = true };


                checkPoints.PresentChoices(
                    string.Format("You've already completed {0} today.",checkPoint.Name),
                    "Would yould you like to add another occurrence?",
                    new[]{ ok, no, cancel });

			} else {
			 	created = checkPoints.AddOccurrenceToCheckPoint (checkPoint, mins);
			}
			return created;
		}
	}
}

