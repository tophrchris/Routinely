using System;
using MonoTouch.Dialog;
using ClockKing.Core;
using Foundation;
using UIKit;

namespace ClockKing
{
	public class CheckPointElement:OwnerDrawnElement
	{
		CheckPoint data {get;set;}
		CheckPointController controller {get;set;}
		CheckPointTableCell cell;
		bool CommandsAttached=false;
	

		public CheckPointElement (CheckPoint source,CheckPointController controller):base(UITableViewCellStyle.Default, "sampleOwnerDrawnElement")
		{
			this.data = source;
			this.controller = controller;
		}

		public CheckPointTableCell GetCell (UIKit.UITableView tv)
		{
			return cell;
		}
		public override void Draw (CoreGraphics.CGRect bounds, CoreGraphics.CGContext context, UIKit.UIView view)
		{
			UIColor.White.SetFill ();
			context.FillRect (bounds);

			if (this.cell == null) {
				this.cell = new CheckPointTableCell ();
				view.AddSubview (cell);
				//cell.Delegate = controller.UtilityButtonHandler;
			}
			cell.RenderCheckpointForDetail(this.data);
			//if (!CommandsAttached) 
			//	CommandsAttached= controller.Commands.AttachUtilityButtonsToCell (cell);

			view.LayoutIfNeeded ();
		}

		public override nfloat Height (CoreGraphics.CGRect bounds)
		{
			return CheckPointTableCell.Height;
		}
	}
}

