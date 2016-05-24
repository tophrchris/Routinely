using System;
using UIKit;
using MonoTouch.Dialog;
using ClockKing.Model;
using ClockKing.Extensions;

namespace ClockKing
{
	public class AddHistoricInstanceDialog:DialogViewController
	{
		public CheckPointController Controller;
		private CheckPoint checkPoint;

		private UIDatePicker picker { get; set; }
		private BooleanElement nowSwitch{ get; set; }


		public AddHistoricInstanceDialog (
			CheckPointController controller, 
			RootElement root, 
			CheckPoint checkpoint, 
			bool pushing
		) : base (root, pushing)
		{
			this.Controller = controller;
			this.checkPoint = checkpoint;

			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.DateAndTime };
			this.picker.MaximumDate = DateTime.UtcNow.ToNSDate();

			var pickerElement = new UIViewElement (string.Empty, this.picker, false);
			this.nowSwitch = new BooleanElement ("default to current time:", true);

			picker.ValueChanged += (s, e) => nowSwitch.Value=false;

			nowSwitch.ValueChanged += (s, e) => {
				if (nowSwitch.Value)
					this.picker.Date = DateTime.UtcNow.ToNSDate ();
			};


			this.Root.Add (new Section ("checkpoint info")
				{
					new StringElement("Name",checkpoint.Name),
					new StringElement("Target Time",
						(DateTime.Today.Date+checkpoint.TargetTime).ToString("t")),
					new StringElement("Last Occurrence",checkpoint.SinceLastOccurrence.ToString("g"))
				}
			);
			this.Root.Add (new Section ("occurrence")
				{
					new MultilineElement("specify the time of the occurrence:"),
					pickerElement,
					nowSwitch});


			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,(s,e)=>this.Save()),true);

		}

		public bool Save()
		{
			this.Controller.AddOccurrenceToCheckPoint (this.checkPoint,
				this.picker.Date.ToDateTime ().ToLocalTime ());

			this.Controller.NavigationController.PopViewController (true);
			return true;
		}


	}
}

