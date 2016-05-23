using System;
using MonoTouch.Dialog;
using Foundation;
using UIKit;

namespace ClockKing
{
	public class AddNewCheckpointDialog:DialogViewController
	{
		CheckPointController Controller{ get; set; }

		private EntryElement nameElement { get; set; }
		private TimeElement targetElement { get; set; }
		private EntryElement emojiElement { get; set; }
		private BooleanElement nowElement { get; set; }

		public AddNewCheckpointDialog (CheckPointController controller, RootElement root, bool pushing) : base (root, pushing)
		{
			this.Controller = controller;


			var section = new Section ("New Checkpoint:");
			this.nameElement = new EntryElement ("name", "Name your checkpoint", "");
			var instructions = new MultilineElement ("specify the time that you expect to complete this checkpoint, each day:");
			this.emojiElement = new EntryElement ("Emoji", "specify some emoji :P","");
			this.targetElement = new TimeElement ("target", DateTime.Now);
			this.nowElement = new BooleanElement ("Add Occurrence now?", false);

			section.AddAll (new Element[]{ 	
				nameElement,
				instructions,
				emojiElement,
				targetElement,
				nowElement });
			
			this.Root.Add(section);

			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,(s,e)=>this.Save()),true);

		}

		public bool Save(){
			
			var newcp = this.Controller
					.AddNewCheckPoint (
						nameElement.Value,
						targetElement.DateValue.ToLocalTime ().TimeOfDay,
						emojiElement.Value);

				if (nowElement.Value){
					var o = newcp.CreateOccurrence();
					newcp.AddOccurrence (o);
					this.Controller.CheckPointData.SaveOccurrence(o);
				}
				
				this.Controller.RespondToModelChanges ();
				CancelDialog ();

			return true;
		}
		public void CancelDialog()
		{
			this.Controller.NavigationController.PopViewController (true);
		}
	}
}

