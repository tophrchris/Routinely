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

		private CheckPointController Controller;

		public CheckPointDataSource (CheckPointController controller)
		{ 
			this.Controller = controller;
		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			if (section == 0)
				return this.Controller.CheckPointData.CheckPointPairs.Count ();
			else
				return this.Controller.CheckPointData.DisabledCheckPoints.Count ();
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			if (this.Controller.CheckPointData.DisabledCheckPoints.Any ())
				return 2;
			else
				return 1;
		}
		public override string TitleForHeader (UITableView tableView, nint section)
		{
			if (!this.Controller.CheckPointData.DisabledCheckPoints.Any ())
				return string.Empty;
			
			if (section == 0)
				return "enabled";
			else
				return "disabled";
			
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			//would love to get this from CheckPointTableCell?
			return 71;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			CheckPoint found = GetCheckpoint (indexPath);
		
			var cell = CheckPointTableCellFactory (tableView, found);

			return cell;

		}

		CheckPointTableCell CheckPointTableCellFactory (UITableView tableView, CheckPoint cpe)
		{
			var cell = tableView.DequeueReusableCell (CheckPointTableCell.Key) as CheckPointTableCell;

			var utilButtons = this.Controller.UtilityButtonHandler;
			if (cell == null)
				cell = new CheckPointTableCell () {Delegate = utilButtons};

			cell.CheckPoint = cpe;
			utilButtons.AttachUtilityButtonsToCell (cell);
			cell.RenderCheckpoint (cpe);
			return cell;
		}
			
		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			this.Controller.Detail.ShowDetailDialog (GetCheckpoint(indexPath));
		}

		private IEnumerable<CheckPoint> GetCheckpointsForSection(int section)
		{
			if (section == 0)
				return this.Controller.CheckPointData.CheckPointPairs.Select (cpp => cpp.firstEvent);
			else
				return this.Controller.CheckPointData.DisabledCheckPoints;
		}
		private CheckPoint GetCheckpoint(NSIndexPath path)
		{
			return GetCheckpointsForSection (path.Section).ElementAt (path.Row);
		}


		public override void AccessoryButtonTapped (UITableView tableView, NSIndexPath indexPath)
		{

			var f = GetCheckpoint(indexPath);

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
	}
}

