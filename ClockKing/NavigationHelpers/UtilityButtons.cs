using System;
using ClockKing.Model;
using UIKit;
using Foundation;

namespace ClockKing
{
	public class DisableCheckPointButton:UtilityButton
	{
		public DisableCheckPointButton():base(UIColor.DarkGray,"Disable")
		{
		}
		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			checkPoint.Enabled = false;
			return true;
		}
	}

	public class EnableCheckPointButton:UtilityButton
	{
		public EnableCheckPointButton():base(UIColor.LightGray,"Enable")
		{
		}
		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			checkPoint.Enabled = true;
			return true;
		}
	}

	public class AddOccurrenceButton:UtilityButton
	{
		public AddOccurrenceButton():base(UIColor.Green,"Add"){
		}
		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			var o = checkPoint.CreateOccurrence ();
			checkPoint.AddOccurrence (o);
			MsgBox ("added!", o.timeStamp.ToString ("t"));
			return true;
		}
	}

	public class AddHistoricOccurrenceButton:UtilityButton
	{
		public AddHistoricOccurrenceButton():base(UIColor.Orange,"Add..."){
		}
		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			var o = checkPoint.CreateOccurrence ();
			checkPoint.AddOccurrence (o);
			MsgBox ("added!", o.timeStamp.ToString ("t"));
			return true;
		}
	}

	public class DeleteCheckPointButton:UtilityButton
	{
		public DeleteCheckPointButton():base(UIColor.Red,"delete"){
		}
		public override bool ExecuteFor (CheckPointController controller, CheckPoint checkPoint)
		{
			
			var okCancelAlertController = UIAlertController.Create("Please confirm:",
				string.Format("Are you sure you would like to delete the checkpoint, {0}?",checkPoint.Name)
					, UIAlertControllerStyle.ActionSheet);
			var okAction = UIAlertAction.Create ("yes, delete!", UIAlertActionStyle.Default, (alert)=>PerformDeletion(controller,checkPoint));
			okCancelAlertController.AddAction(okAction);
			okCancelAlertController.AddAction(UIAlertAction.Create("nevermind", UIAlertActionStyle.Cancel, null));
			controller.PresentModalViewController(okCancelAlertController, true);

			return false;

		}
		private bool PerformDeletion(CheckPointController controller, CheckPoint checkpoint)
		{
			var deleted =  controller.CheckPointData.RemoveCheckPoint (checkpoint);
			if(deleted)
				controller.ReloadData ();
			return deleted;
		}
	}
}


