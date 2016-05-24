using System;
using MonoTouch.Dialog;
using Foundation;
using UIKit;
using ClockKing.Extensions;
using System.Linq;

namespace ClockKing
{
	public class AddNewCheckpointDialog:DialogViewController
	{
		CheckPointController Controller{ get; set; }

		private EntryElement nameElement { get; set; }
		private TimeElement targetElement { get; set; }
		private EntryElement emojiElement { get; set; }
		private BooleanElement nowElement { get; set; }
		private UIDatePicker picker { get; set; }
		private BooleanElement nowSwitch{ get; set; }

		public AddNewCheckpointDialog (CheckPointController controller, RootElement root, bool pushing) : base (root, pushing)
		{
			this.Controller = controller;
			this.Style = UITableViewStyle.Grouped;

			this.nameElement = new EntryElement ("Name", "Name your checkpoint", "");
			this.emojiElement = new EntryElement ("Abbreviation", "a short (2-letter) name","");
			this.nowElement = new BooleanElement ("Add Occurrence now?", false);
			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.Time };
			this.nowSwitch = new BooleanElement ("default to current time:", true);

			var instructions = new MultilineElement ("What time do you expect to complete this checkpoint, each day?");
			var pickerWrapper = new UIViewElement (string.Empty, picker, false);



			picker.ValueChanged += (s, e) => nowSwitch.Value=false;

			nowSwitch.ValueChanged += (s, e) => {
				if (nowSwitch.Value)
					this.picker.Date = DateTime.UtcNow.ToNSDate ();
			};

			this.Root.Add(new Section ("New Checkpoint:")
				{ 	
					nameElement,
					emojiElement,
					instructions,
					pickerWrapper,
					nowSwitch,
					nowElement 
				});
					
			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,(s,e)=>this.Save()),true);
		}

		public bool Save(){
			
			var newcp = this.Controller
					.AddNewCheckPoint (
						nameElement.Value,
					picker.Date.ToDateTime().ToLocalTime().TimeOfDay,
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

