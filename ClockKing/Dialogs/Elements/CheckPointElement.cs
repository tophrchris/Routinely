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
		iCheckpointCommandController controller {get;set;}
		CheckPointTableCell cell;
	

		public CheckPointElement (CheckPoint source):base(UITableViewCellStyle.Default, "sampleOwnerDrawnElement")
		{
			this.data = source;
		}


		public override void Draw (CoreGraphics.CGRect bounds, CoreGraphics.CGContext context, UIKit.UIView view)
		{
			UIColor.White.SetFill ();
			context.FillRect (bounds);

			if (this.cell == null) {
				this.cell = new CheckPointTableCell ();
				view.AddSubview (cell);
			}
			cell.RenderCheckpointForDetail(this.data);

			view.LayoutIfNeeded ();
		}

		public override nfloat Height (CoreGraphics.CGRect bounds)
		{
			return CheckPointTableCell.Height;
		}
	}
}

