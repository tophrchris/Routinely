﻿using System;
using ClockKing.Core;
using UIKit;

namespace ClockKing
{
	public class AddOccurrenceCommand:EnabledCheckpointCommand
	{
		public AddOccurrenceCommand(UIColor color,string label):base(color,label){}
		public AddOccurrenceCommand():base(UIColor.Green,"Add")
		{
			this.ChangesCheckpoint = false;
			this.Category = "Right";
			this.LongName = "Add an occurrence right now";
		}

		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			var o = AddOccurrenceToCheckpoint (controller, checkPoint);
			var added = o != null;
			return added;
		}

		protected Occurrence AddOccurrenceToCheckpoint(CheckPointController controller, CheckPoint checkPoint,int mins=0)
		{
			return controller.AddOccurrenceToCheckPoint (checkPoint, mins);
		}
	}
}

