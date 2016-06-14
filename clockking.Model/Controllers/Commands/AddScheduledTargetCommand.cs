using System;
using System.Collections.Generic;
using System.Linq;
using ClockKing.Core;

namespace ClockKing
{
	public class AddScheduledTargetCommand:EnabledCheckpointCommand,IDialogBoundCommand
	{
		public iNavigatableDialog ExistingDialog{ get; set; }

		public AddScheduledTargetCommand():base("Blue","Alt Target")
		{
			this.Category = "In Place";
			this.LongName = "Add Alternative Target";
		}

		public AddScheduledTargetCommand (iNavigatableDialog existing):this()
		{
			this.ExistingDialog = existing;
		}

		public override bool ExecuteFor (iCheckpointCommandController controller, ClockKing.Core.CheckPoint checkPoint)
		{
			controller.PresentScheduledTargetDialogForCheckpoint (checkPoint, ExistingDialog);
			return false;
		}
	}
}

