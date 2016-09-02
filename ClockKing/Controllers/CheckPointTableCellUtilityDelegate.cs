using System;
using Foundation;
using UIKit;
using SWTableViewCells;
using System.Collections.Generic;
using ClockKing.Core;
using System.Linq;

namespace ClockKing
{

	public class CheckPointTableCellUtilityDelegate : SWTableViewCellDelegate
	{
		private CheckPointController Controller{ get; set; }

		public CheckPointTableCellUtilityDelegate(CheckPointController controller)
		{
			this.Controller = controller;
		}
		public override bool ShouldHideUtilityButtonsOnSwipe (SWTableViewCell cell)
		{
			return true;
		}

		public override void DidEndScrolling (SWTableViewCell cell)
		{
			//TODO: this is where we could auto do-someting?
			System.Diagnostics.Debug.WriteLine("scrolling ended?");
		}

		public override void DidTriggerLeftUtilityButton (SWTableViewCell cell, nint index)
		{
			var cpp = ((CheckPointTableCell)cell).CheckPoint;
			this.ExecuteCommandForCheckpoint (cpp, cell.LeftUtilityButtons, (int)index);
		}

		public override void DidTriggerRightUtilityButton (SWTableViewCell cell, nint index)
		{
			var cpp = ((CheckPointTableCell)cell).CheckPoint;
			this.ExecuteCommandForCheckpoint (cpp, cell.RightUtilityButtons, (int)index);
		}

		private bool ExecuteCommandForCheckpoint(CheckPoint checkPoint, IEnumerable<UIButton> buttons, int triggeredButtonIndex)
		{
			var triggeredUtility = buttons.ElementAt (triggeredButtonIndex).Title(UIControlState.Application);
			var foundUtil = this.Controller.Commands.Commands [triggeredUtility];
			var checkPoints = this.Controller.CheckPoints;

			(UIApplication.SharedApplication.Delegate as AppDelegate).Track("swipe", foundUtil.Name, checkPoint.Name);

			var executed=  foundUtil.ExecuteFor (checkPoints, checkPoint);

			if (executed && foundUtil.ChangesCheckpoint)
				checkPoints.ResaveCheckpoints ();
			
			return executed ;
		}
	}
}

