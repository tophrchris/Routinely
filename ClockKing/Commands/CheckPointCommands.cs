using System;
using ClockKing.Model;
using UIKit;
using Foundation;

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
		public AddOccurrenceCommand():base(UIColor.Green,"Add")
		{
			this.Category = "Right";
			this.LongName = "Add an occurrence right now";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			var o = checkPoint.CreateOccurrence ();
			checkPoint.AddOccurrence (o);
			controller.CheckPointData.SaveOccurrence (o);
			MsgBox ("added!", o.timeStamp.ToString ("t"));
			return true;
		}
	}

	public class AddHistoricOccurrenceCommand:EnabledCheckpointCommand
	{
		public AddHistoricOccurrenceCommand():base(UIColor.Orange,"Add...")
		{
			this.Category = "Right";
			this.LongName = "Add an occurrence in the past";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			var o = checkPoint.CreateOccurrence ();
			checkPoint.AddOccurrence (o);
			MsgBox ("added!", o.timeStamp.ToString ("t"));
			return true;
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
			var okAction = UIAlertAction.Create ("yes, delete!", UIAlertActionStyle.Default, (alert)=>PerformDeletion(controller,checkPoint));
			okCancelAlertController.AddAction(okAction);
			okCancelAlertController.AddAction(UIAlertAction.Create("nevermind", UIAlertActionStyle.Cancel, null));
			controller.PresentModalViewController(okCancelAlertController, true);

			return false;//always return false because the deletion callback will take care of reloading
		}
			
		private bool PerformDeletion(CheckPointController controller, CheckPoint checkpoint)
		{
			var deleted = controller.CheckPointData.RemoveCheckPoint (checkpoint);
			if(deleted)
				controller.ReloadData ();
			return deleted;
		}
	}
}


