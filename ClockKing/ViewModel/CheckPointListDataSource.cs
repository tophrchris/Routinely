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
		private string[] SectionNames = { "Active Checkpoints", "Disabled Checkpoints" };

		public CheckPointDataSource (CheckPointController controller)
		{ 
			this.Controller = controller;
		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return GetCheckpointsForSection((int)section).Count ();
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			if (this.Controller.CheckPointData.DisabledCheckPoints.Any ())
				return 2;
			return 1;
		}

		public override string TitleForHeader (UITableView tableView, nint section)
		{
			if (!this.Controller.CheckPointData.DisabledCheckPoints.Any ())
				return string.Empty;
			
			return SectionNames [section];	
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			//would love to get this from CheckPointTableCell?
			return 71;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
				return CheckPointTableCellFactory (tableView, GetCheckpoint (indexPath));
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			this.Controller.Detail.ShowDetailDialog (GetCheckpoint(indexPath));
		}

		CheckPointTableCell CheckPointTableCellFactory (UITableView tableView, CheckPoint checkpoint)
		{
			var cell = tableView.DequeueReusableCell (CheckPointTableCell.Key) as CheckPointTableCell;

			if (cell == null)
				cell = new CheckPointTableCell () 
					{Delegate = this.Controller.UtilityButtonHandler};

			cell.RenderCheckpoint (checkpoint);
			Controller.Commands.AttachUtilityButtonsToCell (cell);
			return cell;
		}
			
		public CheckPoint GetCheckpoint(NSIndexPath path)
		{
			return GetCheckpointsForSection(path.Section).ElementAt (path.Row);
		}

		private IEnumerable<CheckPoint> GetCheckpointsForSection(int section)
		{
			if (section == 0)
				return this.Controller.CheckPointData.CheckPointPairs.Select (cpp => cpp.firstEvent);
			else
				return this.Controller.CheckPointData.DisabledCheckPoints;
		}
	}
}

