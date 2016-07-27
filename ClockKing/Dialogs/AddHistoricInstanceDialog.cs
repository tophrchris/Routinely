using System;
using UIKit;
using MonoTouch.Dialog;
using ClockKing.Core;
using ClockKing.Extensions;
using Humanizer;
using System.Diagnostics;

namespace ClockKing
{
	public class AddHistoricInstanceDialog:DialogViewController,iNavigatableDialog
	{
		public iCheckpointCommandController CheckPoints;
		private CheckPoint checkPoint;

		private UIDatePicker picker { get; set; }
		private BooleanElement nowSwitch{ get; set; }


		public AddHistoricInstanceDialog (iCheckpointCommandController checkPoints, RootElement root, CheckPoint checkpoint, bool pushing) : base (root, pushing)
		{
			this.CheckPoints = checkPoints;
			this.checkPoint = checkpoint;

			this.picker = new UIDatePicker (){ Mode = UIDatePickerMode.DateAndTime };
			this.picker.MaximumDate = DateTime.UtcNow.ToNSDate();

			var pickerElement = new UIViewElement (string.Empty, this.picker, false);

			this.picker.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;

			this.nowSwitch = new BooleanElement ("Reset to current time:", true);


			picker.ValueChanged += (s, e) => nowSwitch.Value=false;

			nowSwitch.ValueChanged += (s, e) => {
				if (nowSwitch.Value)
					this.picker.Date = DateTime.UtcNow.ToNSDate ();
			};


			this.Root.Add(new CheckPointCellSection(this.checkPoint));

			this.Root.Add (new Section ("Occurrence")
				{
					new MultilineElement("Specify the time of the occurrence:"),
					pickerElement,
					nowSwitch});

			this.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Save,(s,e)=>this.Save()),true);
		}

		public bool Save()
		{

			Debug.WriteLine("hid save");

			//TODO: this needs to hook into the "already addded" logic in the command?

			this.CheckPoints.AddOccurrenceToCheckPoint (this.checkPoint,
				this.picker.Date.ToDateTime ().ToLocalTime ());

			this.ResetNavigation ();
			return true;
		}

		#region iNavigatableDialog implementation

		public void ResetNavigation (bool refreshData=false)
		{
			Debug.WriteLine("hid reset nav");
			((iNavigatableDialog)this.CheckPoints).ResetNavigation(refreshData);
		}

		#endregion
	}
}

