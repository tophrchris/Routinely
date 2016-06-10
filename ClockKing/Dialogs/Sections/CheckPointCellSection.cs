using System;
using MonoTouch.Dialog;
using ClockKing.Core;

namespace ClockKing
{
	public class CheckPointCellSection:Section
	{
		public CheckPointCellSection (CheckPointController controller, CheckPoint checkpoint)
		{
			var tableCell = new CheckPointElement (checkpoint,controller);
			this.Caption = "Goal:";
			this.Add (tableCell);
		}
	}
}

