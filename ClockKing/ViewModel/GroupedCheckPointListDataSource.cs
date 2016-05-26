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
	public class GroupedCheckPointDataSource:UITableViewSource
	{

		private CheckPointController Controller {get;}
		private DataModel checkPointData { get; }
	
		public GroupedCheckPointDataSource (CheckPointController controller,DataModel data)
		{ 
			this.Controller = controller;
			this.checkPointData = data;
		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return GetCheckpointsForSection((int)section).Count ();
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return this.checkPointData.GroupedCheckPoints.Keys.Count;
		}

		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return this.checkPointData.GroupedCheckPoints.Keys.ToArray().ElementAt((int)section);
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
			this.Controller.ShowDetailDialogFor(GetCheckpoint(indexPath));
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

		private LinkedList<CheckPoint> GetCheckpointsForSection(int section)
		{
			return this.checkPointData.GroupedCheckPoints.ElementAt (section).Value;
		}
	}
}

