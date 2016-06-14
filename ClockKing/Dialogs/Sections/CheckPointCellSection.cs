using System;
using MonoTouch.Dialog;
using ClockKing.Core;

namespace ClockKing
{
	public class CheckPointCellSection:Section
	{
		public CheckPointCellSection (CheckPoint checkpoint)
		{
			var tableCell = new CheckPointElement (checkpoint);
			this.Caption = "Goal:";
			this.Add (tableCell);
		}
	}
}

