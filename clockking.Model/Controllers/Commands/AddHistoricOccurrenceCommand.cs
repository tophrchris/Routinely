using System;
using ClockKing.Core;
using System.Linq;
using Humanizer;
using EmojiSharp;

namespace ClockKing
{
	public class AddHistoricOccurrenceCommand:AddOccurrenceCommand
	{
		public AddHistoricOccurrenceCommand():base("Orange","Done...")
		{
			this.Category = "Right";
			this.LongName = "Add an occurrence in the past";
		}



        public override string EmojiName { 
            get 
            {
                var now = DateTime.Now.ToLocalTime ();
                var hour = now.Hour;
                if (hour > 12)
                    hour -= 12;
                var past30 = (now.Minute >= 30)?"30":"";
                var emojiString = "clock{0}{1}".FormatWith (hour, past30);
                return emojiString;
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

			var choices = new[]{ 15, 30, 60, 90 }.Select (i =>
				new ModalChoice () {
				Label = string.Format ("{0} ago- {1}", i.Minutes ().Humanize (2), DateTime.Now.AddMinutes (i * -1).ToString ("t")),
				Handler = () => adder (i * -1)
			}).ToList ();


			choices.Add(new ModalChoice(){Label="Custom...",Handler=()=>this.ShowCustomDialog(controller,checkPoint)});
			choices.Add(new ModalChoice(){Label="Nevermind",Cancel=true});

			controller.PresentChoices ("Add", this.LongName, choices);

			return false;
		}
	
		public void ShowCustomDialog(iCheckpointCommandController controller, CheckPoint checkPoint)
		{
            controller.PresentHistoricOccurrenceDialogFor(checkPoint);
		}
	}

}

