using System;
using UIKit;
using MonoTouch.Dialog;
using System.Linq;

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

		public override bool ExecuteFor (CheckPointController controller, ClockKing.Core.CheckPoint checkPoint)
		{
			var editDialog = new CheckPointEditingDialog(controller,new RootElement("Edit"),true);
			editDialog.RenderForCheckPoint(checkPoint);
			controller.NavigationController.PushViewController(editDialog,true);

			return false;
		}
		public override bool ShouldDecorate (ClockKing.Core.CheckPoint toDecorate)
		{
			return true;
		}
	}

	public class InPlaceEditCheckPointCommand:Command
	{
		public InPlaceEditCheckPointCommand(UIColor Color, string Label):base(Color,Label){}

		public InPlaceEditCheckPointCommand(DialogViewController existing):this()
		{
			this.ExistingDialog = existing;
		}

		public InPlaceEditCheckPointCommand ():base(UIColor.Magenta,"Edit")
		{
			this.Category = "In Place";
			this.LongName = "Edit Goal";
		}

		public DialogViewController ExistingDialog{ get; set;}

		public override bool ExecuteFor (CheckPointController controller, ClockKing.Core.CheckPoint checkPoint)
		{
			
			var editDialog = new CheckPointEditingDialog(controller,new RootElement("Edit"),true);
			editDialog.RenderForCheckPoint(checkPoint);
			ExistingDialog.Root.Insert (0,editDialog.Root.First());

			ExistingDialog.NavigationItem.SetLeftBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Cancel,
				(s,e)=>
				{
					UIView.Animate(.25d,()=> ExistingDialog.Root.RemoveAt(0));
					//TODO: make resetable an interface?
					((CheckPointDetailDialog) ExistingDialog).ResetNavigation();
				}),true);

			ExistingDialog.NavigationItem.SetRightBarButtonItem 
			(editDialog.NavigationItem.RightBarButtonItem,true);

			return false;
		}
		public override bool ShouldDecorate (ClockKing.Core.CheckPoint toDecorate)
		{
			return true;
		}
	}
}

