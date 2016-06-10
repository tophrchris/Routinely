using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using SWTableViewCells;

namespace ClockKing
{
	public class GroupedCheckPointDataSource:UITableViewSource
	{

		private CheckPointController Controller {get;}
		private DataModel checkPointData { get; }
		private Dictionary<GroupingChoices,CheckPointGrouper> groupers { get; set; }
	
		public GroupedCheckPointDataSource (CheckPointController controller,DataModel data)
		{ 
			this.Controller = controller;
			this.checkPointData = data;
			this.groupers = new Dictionary<GroupingChoices, CheckPointGrouper> () 
			{
				{GroupingChoices.ByTimeOfDay,new GroupCheckPointsByTimeOfDay (this.checkPointData.checkPoints.Values)},
				{GroupingChoices.ByStatus,new GroupCheckPointsByStatus (this.checkPointData.checkPoints.Values)}
			};
		}

		public override nint RowsInSection (UITableView tableView, nint section)
		{
			return GetCheckpointsForSection((int)section).Count ();
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			return this.GroupedCheckPoints.Count ();
		}

		public override string TitleForHeader (UITableView tableView, nint section)
		{
			return this.GroupedCheckPoints.ElementAt((int)section).Key;
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return CheckPointTableCell.Height;
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

			var key =  CheckPointTableCell.Key;
			var cell = tableView.DequeueReusableCell (key) as CheckPointTableCell;

			if (cell == null) {
				cell =  new CheckPointTableCell ();
				cell.Delegate = this.Controller.UtilityButtonHandler;
			}

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
			return this.GroupedCheckPoints.ElementAt(section).Value;
		}
		private IEnumerable<KeyValuePair<string,IEnumerable<CheckPoint>>> GroupedCheckPoints
		{
			get
			{
				return this.groupers [this.Controller.Options.GroupingChoice].GroupedCheckPoints;
			}
		}
	}
}