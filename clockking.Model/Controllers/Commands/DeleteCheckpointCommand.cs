using System;
using ClockKing.Core;

namespace ClockKing
{
	public class DeleteCheckPointCommand:DisabledCheckpointCommand
	{
		public DeleteCheckPointCommand():base("Red","Delete")
		{
			this.IsDestructive = true;
			this.Category = "Left";
		}

		public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
		{

			controller.PresentConfirmationDialog(
				()=>controller.RemoveCheckpoint(checkPoint),
				"Please confirm:",
				string.Format("Are you sure you would like to delete the checkpoint, {0}?",checkPoint.Name),
				"yes, delete!"
				);

			return false;
		}
	}
}

