using System;
using ClockKing.Model;
using UIKit;

namespace ClockKing
{
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

