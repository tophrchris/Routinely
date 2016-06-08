using System;
using ClockKing.Core;
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

			var okCancelAlertController = SharedDialogs.ConfirmationDialog(
				(alert)=>
				{
					if(controller.RemoveCheckpoint(checkPoint))
						controller.ResetNavigation();
				},
				"Please confirm:",
				string.Format("Are you sure you would like to delete the checkpoint, {0}?",checkPoint.Name),
				"yes, delete!",
				"Nevermind"
				);
					
			controller.PresentModalViewController(okCancelAlertController, true);

			return false;//always return false because the deletion callback will take care of reloading
		}
	}
}

