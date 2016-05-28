using System;
using ClockKing.Model;
using UIKit;
using System.Linq;

namespace ClockKing
{
	public class AddHistoricOccurrenceCommand:AddOccurrenceCommand
	{
		public AddHistoricOccurrenceCommand():base(UIColor.Orange,"Add...")
		{
			this.Category = "Right";
			this.LongName = "Add an occurrence in the past";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{

			var ac = CreateActionSheet (controller, checkPoint);

			ac.AddAction (UIAlertAction.Create ("Custom...", UIAlertActionStyle.Default,
				(a)=>this.ShowCustomDialog(controller,checkPoint)));
			ac.AddAction (UIAlertAction.Create ("nevermind", UIAlertActionStyle.Cancel, null));

			controller.PresentViewController (ac, true, null);

			return true;
		}
		public UIAlertController CreateActionSheet(CheckPointController controller,CheckPoint checkPoint)
		{
			Action<int> adder = (n) =>{
				AddOccurrenceToCheckpoint (controller, checkPoint, n);
			};

			var choices = new[]{ 15, 30, 60, 90 }.Select (i =>
				UIAlertAction.Create (
					string.Format ("{0} mins ago", i),
					UIAlertActionStyle.Default,
					a=>adder(i*-1)
				));
			var ac = UIAlertController.Create("Add",this.LongName,UIAlertControllerStyle.ActionSheet);

			foreach (var a in choices)
				ac.AddAction (a);

			return ac;
		}
		public void ShowCustomDialog(CheckPointController controller, CheckPoint checkPoint)
		{
			var custom = new AddHistoricInstanceDialog(controller,new MonoTouch.Dialog.RootElement("Add Occurrence"),checkPoint,true);
			controller.NavigationController.PushViewController (custom, true);
		}
	}

}

