using System;
using UIKit;
using MonoTouch.Dialog;

namespace ClockKing
{
	public class EditCheckPointCommand:Command
	{
		public EditCheckPointCommand(UIColor Color, string Label):base(Color,Label){}

		public EditCheckPointCommand ():base(UIColor.Magenta,"Edit")
		{
			this.Category = "Left";
			this.LongName = "Edit Goal";
		}

		public override bool ExecuteFor (CheckPointController controller, ClockKing.Model.CheckPoint checkPoint)
		{
			var editDialog = new CheckPointEditingDialog(controller,new RootElement("Edit"),true);
			editDialog.RenderForCheckPoint(checkPoint);
			controller.NavigationController.PushViewController(editDialog,true);

			return true;
		}
		public override bool ShouldDecorate (ClockKing.Model.CheckPoint toDecorate)
		{
			return true;
		}
	}
}

