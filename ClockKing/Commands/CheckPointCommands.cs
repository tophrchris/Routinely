using System;
using ClockKing.Model;
using UIKit;
using Foundation;
using System.Linq;

namespace ClockKing
{

	public class DisableCheckPointCommand:EnabledCheckpointCommand
	{
		public DisableCheckPointCommand():base(UIColor.DarkGray,"Disable")
		{
			this.Category = "Left";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			checkPoint.Enabled = false;
			return true;
		}
	}

	public class EnableCheckPointCommand:DisabledCheckpointCommand
	{
		public EnableCheckPointCommand():base(UIColor.LightGray,"Enable")
		{
			this.Category = "Right";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			checkPoint.Enabled = true;
			return true;
		}
	}

	public class AddOccurrenceCommand:EnabledCheckpointCommand
	{
		public AddOccurrenceCommand(UIColor color,string label):base(color,label){}
		public AddOccurrenceCommand():base(UIColor.Green,"Add")
		{
			this.Category = "Right";
			this.LongName = "Add an occurrence right now";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			var o = AddOccurrenceToCheckpoint (controller, checkPoint);
			MsgBox ("added!", o.timeStamp.ToString ("t"));
			return true;
		}

		protected Occurrence AddOccurrenceToCheckpoint(CheckPointController controller, CheckPoint checkPoint,int mins=0)
		{
			return controller.AddOccurrenceToCheckPoint (checkPoint, mins);
		}
	}

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

			ac.AddAction (UIAlertAction.Create ("Custom...", UIAlertActionStyle.Default, (a)=>MsgBox("Custom Date","coming soon!")));
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
	}

	public class DeleteCheckPointCommand:DisabledCheckpointCommand
	{
		public DeleteCheckPointCommand():base(UIColor.Red,"Delete")
		{
			this.IsDestructive = true;
			this.Category = "Left";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			var okCancelAlertController = UIAlertController.Create("Please confirm:",
				string.Format("Are you sure you would like to delete the checkpoint, {0}?",checkPoint.Name)
					, UIAlertControllerStyle.ActionSheet);

			var okAction = UIAlertAction.Create (
				"yes, delete!",
				UIAlertActionStyle.Destructive, 
				(alert)=>controller.RemoveCheckpoint(checkPoint));
			
			okCancelAlertController.AddAction(okAction);
			okCancelAlertController.AddAction(UIAlertAction.Create("nevermind", UIAlertActionStyle.Cancel, null));
			controller.PresentModalViewController(okCancelAlertController, true);

			return false;//always return false because the deletion callback will take care of reloading
		}
	}
}


