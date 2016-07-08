using Foundation;
using System;
using UIKit;

namespace ClockKing.RoutinelyExtension
{
	public partial class GoalTableRow : NSObject
	{
		public GoalTableRow(IntPtr handle) : base(handle)
		{
		}

		public string  Label { 
			get {return ""; } 
			set {this.GoalLabel.SetText(value); } 
		}
    }
}