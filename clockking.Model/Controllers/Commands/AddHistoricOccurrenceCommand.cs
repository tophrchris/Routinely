using System;
using ClockKing.Core;
using System.Linq;
using Humanizer;
using EmojiSharp;

namespace ClockKing
{
	public class AddHistoricOccurrenceCommand:AddOccurrenceCommand
	{
		public AddHistoricOccurrenceCommand():base("LightBlue","Earlier...")
		{
			this.Category = "Right";
			this.LongName = "Add an earlier completion";
		}



        public override string EmojiName { 
            get 
            {
                return Emoji.NowClock.ShortName;
            }
            set 
            {
                throw new NotImplementedException ();
            } 
        }

		public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
		{
			Action<int> adder = (n) =>{
				AddOccurrenceToCheckpoint (controller, checkPoint, n);
			};

			var choices = new[]{ 15, 30, 45, 60,120 }.Select (i =>
				new ModalChoice () {
                Label = string.Format ("{2} {0} ago- {1}", 
                                       i.Minutes ().Humanize (2),
                                       DateTime.Now.AddMinutes (i * -1).ToString ("t"),
                                       Emoji.ClockFor(DateTime.Now.AddMinutes (i * -1)).Unified
                                      ),
				Handler = () => adder (i * -1)
			}).ToList ();


			choices.Add(new ModalChoice(){Label="Specify a time...",Handler=()=>this.ShowCustomDialog(controller,checkPoint)});
			choices.Add(new ModalChoice(){Label="Nevermind",Cancel=true});

            controller.PresentChoices ("Earlier completion of " + checkPoint.Name, "When did you complete this goal?", choices);

			return false;
		}
	
		public void ShowCustomDialog(iCheckpointCommandController controller, CheckPoint checkPoint)
		{
            controller.PresentHistoricOccurrenceDialogFor(checkPoint);
		}
	}

}

