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

		public static NSString CheckPointListCellId = new NSString ("CheckPointListCellId");

		public CheckPointDataSource (CheckPointTableViewController controller)
		{ 
			this.Controller = controller;
			this.Detail=new CheckpointDetailCommand(this.Controller);
		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return this.Controller.CheckPointData.CheckPointPairs.Count();
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (CheckPointListCellId, indexPath);
		
			var checkPoints = this.Controller.CheckPointData.CheckPointPairs.ElementAt (indexPath.Row);

			this.Controller.DecorateCell (cell, checkPoints);

			return cell;
		}
			
		public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
		{

			var f = this.
				Controller.
				CheckPointData.
				CheckPointPairs.ElementAt (indexPath.Row).firstEvent;

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
			this.Detail.ShowDetailDialog (this.Controller.CheckPointData.CheckPointPairs.ElementAt(indexPath.Row));
		}
	}
}

