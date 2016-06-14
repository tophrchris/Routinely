using System;
using ClockKing.Core;

namespace ClockKing
{
	public class EnableCheckPointCommand:DisabledCheckpointCommand
	{
		public EnableCheckPointCommand():base("LightGray","Enable")
		{
			this.Category = "Right";
		}

		public override bool ExecuteFor (iCheckpointCommandController controller, CheckPoint checkPoint)
		{
			checkPoint.Enabled = true;
			return true;
		}
	}
}

