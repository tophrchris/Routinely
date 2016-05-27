using System;
using ClockKing.Model;
using UIKit;

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
}

