using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;


namespace ClockKing
{
	public class CheckPointDataSource:UITableViewSource
	{

		private CheckPointTableViewController Controller;
		private CheckpointDetailCommand Detail;

		public CheckPointDataSource (CheckPointTableViewController controller)
		{ 
			this.Controller = controller;
			this.Detail=new CheckpointDetailCommand(this.Controller);
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

			if (cell == null)
				cell = new CheckPointTableCell ();
		
			var checkPoints = GetCheckpointPair (indexPath.Row);

			cell.RenderCheckpoint (checkPoints);
			return cell;
		}
			
		public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
		{

			var f = GetCheckpointPair(indexPath.Row).firstEvent;

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
}

