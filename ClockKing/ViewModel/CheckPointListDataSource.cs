using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;
using SWTableViewCells;


namespace ClockKing
{
	public class CheckPointDataSource:UITableViewSource
	{

		private CheckPointTableViewController Controller;
		private CheckpointDetailCommand Detail;
		private CellDelegate Delegate;


		public CheckPointDataSource (CheckPointTableViewController controller)
		{ 
			this.Controller = controller;
			this.Detail=new CheckpointDetailCommand(this.Controller);
			var b = new UIButton (UIButtonType.RoundedRect);
			b.SetTitle ("add", UIControlState.Normal);
			this.Delegate = new CellDelegate ();

		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return this.Controller.CheckPointData.CheckPointPairs.Count();
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			//would love to get this from CheckPointTableCell?
			return 71;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (CheckPointTableCell.Key) as CheckPointTableCell;

			if (cell == null) {
				
				cell = new CheckPointTableCell ();
				cell.Delegate = Delegate;
				cell.RightUtilityButtons = RightButtons ();
				cell.LeftUtilityButtons = LeftButtons ();
			}
		
			var checkPoints = GetCheckpointPair (indexPath);

			cell.RenderCheckpoint (checkPoints);
			return cell;
		}

		static UIButton[] LeftButtons ()
		{
			NSMutableArray bs = new NSMutableArray ();
			bs.AddUtilityButton (UIColor.Red, "delete");
			bs.AddUtilityButton (UIColor.Gray, "disable");
			return NSArray.FromArray<UIButton> (bs);
		}

		static UIButton[] RightButtons ()
		{
			NSMutableArray bs = new NSMutableArray ();
			bs.AddUtilityButton (UIColor.Green, "now");
			bs.AddUtilityButton (UIColor.Yellow, "add");
			//leftUtilityButtons.AddUtilityButton (UIColor.FromRGBA (1.0f, 0.231f, 0.188f, 1.0f), UIImage.FromBundle ("cross.png"));
			//leftUtilityButtons.AddUtilityButton (UIColor.FromRGBA (0.55f, 0.27f, 0.07f, 1.0f), UIImage.FromBundle ("list.png"));
			return NSArray.FromArray<UIButton> (bs);
		}

		public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
		{

			var f = GetCheckpointPair(indexPath).firstEvent;

			UIAlertController okAlertController = 
				UIAlertController.Create (
					"accessory tapped", 
					f.Name,
					UIAlertControllerStyle.ActionSheet);

			okAlertController.AddAction(
					UIAlertAction.Create("OK",
					UIAlertActionStyle.Default,
					null));

			this.Controller.PresentViewController (okAlertController,true,null);
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			this.Detail.ShowDetailDialog (GetCheckpointPair(indexPath));
		}

		private CheckPointPair GetCheckpointPair(NSIndexPath path)
		{
			return this.Controller.CheckPointData.CheckPointPairs.ElementAt (path.Row);
		}

	}


	class CellDelegate : SWTableViewCellDelegate
	{
		

		public override void DidTriggerLeftUtilityButton (SWTableViewCell cell, nint index)
		{
			Console.WriteLine ("Left button {0} was pressed.", index);

			new UIAlertView ("Left Utility Buttons", string.Format ("Left button {0} was pressed.", index), null, "OK", null).Show ();
		}

		public override void DidTriggerRightUtilityButton (SWTableViewCell cell, nint index)
		{
			Console.WriteLine ("Right button {0} was pressed.", index);

			new UIAlertView ("right Utility Buttons", string.Format ("right button {0} was pressed.", index), null, "OK", null).Show ();
		}
		public override bool ShouldHideUtilityButtonsOnSwipe (SWTableViewCell cell)
		{
			// allow just one cell's utility button to be open at once
			return false;
		}


		public override bool CanSwipeToState (SWTableViewCell cell, SWCellState state)
		{
			switch (state) {
			case SWCellState.Left:
				// set to false to disable all left utility buttons appearing
				return true;
			case SWCellState.Right:
				// set to false to disable all right utility buttons appearing
				return true;
			}
			return true;
		}
			
	}
}

