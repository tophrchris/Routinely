using System;
using ClockKing.Core;

namespace ClockKing
{
	public class DisableCheckPointCommand:EnabledCheckpointCommand
	{
		public DisableCheckPointCommand():base("DarkGray","Disable")
		{
			this.Category = "Left";
		}

		public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
		{
			checkPoint.Enabled = false;
			return true;
		}
	}
}

