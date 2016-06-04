using System;
using ClockKing.Core;
using UIKit;

namespace ClockKing
{
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
}

