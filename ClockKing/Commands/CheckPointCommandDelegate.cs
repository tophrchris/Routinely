using System;
using Foundation;
using UIKit;
using SWTableViewCells;
using System.Collections.Generic;
using ClockKing.Model;
using System.Linq;

namespace ClockKing
{

	public class CheckpointCommandDelegate : SWTableViewCellDelegate
	{
		private CheckPointController Controller{ get; set; }

		public CheckpointCommandDelegate(CheckPointController controller)
		{
			this.Controller = controller;
		}

		public override void DidTriggerLeftUtilityButton (SWTableViewCell cell, nint index)
		{
			var cpp = ((CheckPointTableCell)cell).CheckPoint;
			if(this.ExecuteCommandForCheckpoint (cpp, cell.LeftUtilityButtons, (int)index))
				this.Controller.TableView.ReloadData();
		}

		public override void DidTriggerRightUtilityButton (SWTableViewCell cell, nint index)
		{
			var cpp = ((CheckPointTableCell)cell).CheckPoint;
			if(this.ExecuteCommandForCheckpoint (cpp, cell.RightUtilityButtons, (int)index))
				this.Controller.TableView.ReloadData();
		}

		private bool ExecuteCommandForCheckpoint(CheckPoint checkPoint, IEnumerable<UIButton> buttons, int triggeredButtonIndex)
		{
			var triggeredUtility = buttons.ElementAt (triggeredButtonIndex).CurrentTitle;
			var foundUtil = this.Controller.Commands.Commands [triggeredUtility];
			return  foundUtil.ExecuteFor (this.Controller, checkPoint);
		}
	}
}

