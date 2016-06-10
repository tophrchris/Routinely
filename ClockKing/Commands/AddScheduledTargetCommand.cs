using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;

namespace ClockKing
{
	public class AddScheduledTargetCommand:EnabledCheckpointCommand,IDialogBoundCommand
	{
		public DialogViewController ExistingDialog{ get; set; }

		public AddScheduledTargetCommand():base(UIKit.UIColor.Blue,"Alt Target")
		{
			this.Category = "In Place";
			this.LongName = "Add Alternative Target";
		}

		public AddScheduledTargetCommand (CheckPointDetailDialog existing):this()
		{
			this.ExistingDialog = existing;
		}

		public override bool ExecuteFor (CheckPointController controller, ClockKing.Core.CheckPoint checkPoint)
		{
			var e = this.ExistingDialog as CheckPointDetailDialog;
			var n = checkPoint.AddScheduledtarget(null,new List<DayOfWeek>(){DateTime.Today.DayOfWeek});
			var d = new ScheduledTargetDialog(new RootElement("Scheduled Target"),n,checkPoint,controller,e);
			e.NavigationController.PushViewController(d,true);
			return false;
		}
	}
}

