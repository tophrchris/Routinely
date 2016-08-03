using System;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using Humanizer;

namespace ClockKing
{
	public class AddOccurrenceCommand:EnabledCheckpointCommand
	{
		public AddOccurrenceCommand(string color,string label):base(color,label){}
		public AddOccurrenceCommand():base("Blue","Done!")
		{
			this.ChangesCheckpoint = false;
			this.Category = "Right";
			this.LongName = "Complete right now";
            this.EmojiName = EmojiSharp.Emoji.BALLOT_BOX_WITH_CHECK.Name.Replace(" ","_").ToLower();
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
			if (checkPoint.CompletedToday | checkPoint.IsSkipped) {

                string instructions;
                string okPrompt;
                string noPrompt;

                var titleTemplate = "You've {0} {1} today.";
                var actionword = checkPoint.IsSkipped ? "previously skipped" : "already completed";
                var title = string.Format (titleTemplate, actionword, checkPoint.Name);

                var cancel = new ModalChoice () { Label = "Nevermind", Cancel = true };

                ModalChoice [] choices;

                if (checkPoint.IsSkipped)
                {
                    instructions = "Would yould you like to complete, and remove the skip?";
                    okPrompt = "Yes, complete.";
                    var ok = new ModalChoice () {
                        Label = okPrompt,
                        Handler = () => {
                            if (checkPoint.RemoveOccurrences (DateTime.Now))
                                checkPoints.RewriteOccurrences ();
                            created = checkPoints.AddOccurrenceToCheckPoint (checkPoint, mins);
                        }
                    };
                    choices = new ModalChoice [] { ok, cancel };
                }
                else 
                {
                    instructions = "Would yould you like to add another completion?";
                    okPrompt = "Yes, add another.";
                    noPrompt = "No, Replace existing.";

                    var ok = new ModalChoice () {
                        Label = okPrompt,
                        Handler = () => checkPoints.AddOccurrenceToCheckPoint (checkPoint, mins)
                    };
                    var no = new ModalChoice () {
                        Label = noPrompt,
                        Handler = () => {
                            if (checkPoint.RemoveOccurrences (DateTime.Now))
                                checkPoints.RewriteOccurrences ();
                            created = checkPoints.AddOccurrenceToCheckPoint (checkPoint, mins);
                        }
                    };
                    choices = new ModalChoice [] { ok, no, cancel };
                }
               
               
              
                checkPoints.PresentChoices(title,instructions,choices);

			} else {
			 	created = checkPoints.AddOccurrenceToCheckPoint (checkPoint, mins);
			}
			return created;
		}
	}
}

