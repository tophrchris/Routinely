using Foundation;
using System;
using UIKit;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ClockKing
{
	public class GroupedCheckPointDataSource:UITableViewSource
	{

		private CheckPointController Controller {get;}
		private DataModel checkPointData { get; }
		private Dictionary<GroupingChoices,CheckPointGrouper> groupers { get; set; }
		private List<string> instructions;
	
		public GroupedCheckPointDataSource (CheckPointController controller,DataModel data)
		{ 
			this.Controller = controller;
			this.checkPointData = data;
			this.groupers = new Dictionary<GroupingChoices, CheckPointGrouper> () 
			{
				{GroupingChoices.ByTimeOfDay,new GroupCheckPointsByTimeOfDay (this.checkPointData.checkPoints.Values)},
				{GroupingChoices.ByStatus,new GroupCheckPointsByStatus (this.checkPointData.checkPoints.Values)}
			};

			this.instructions = new List<string>()
			{
				"To edit a goal, swipe it to the right; swipe left to complete it.",
				"pull down to add a new goal",
				"press settings to access settings and history"
			};
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			if (section == this.GroupedCheckPoints.Count())
				return instructions.Count;
			
			return GetCheckpointsForSection((int)section).Count ();
		}

		public override nint NumberOfSections (UITableView tableView)
		{
			var sections = this.GroupedCheckPoints.Count()+1;
			Debug.WriteLine(string.Format("found {0} sections",sections));
			return sections;
		}

		public override string TitleForHeader (UITableView tableView, nint section)
		{
			if (section == this.GroupedCheckPoints.Count())
				return "Instructions:";
			
 
			var found = this.GroupedCheckPoints.ElementAt((int)section);
			Debug.WriteLine(string.Format("getting title for {0}, which has {1} goals",found.Key,found.Value.Count()));
			return found.Key;
		}

		public override nfloat GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Section == this.GroupedCheckPoints.Count())
				return 60;
			
			return CheckPointTableCell.Height;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Section == this.GroupedCheckPoints.Count())
				return GetInstructionCell(indexPath);
			
			return CheckPointTableCellFactory (tableView, GetCheckpoint (indexPath));
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			this.Controller.ShowDetailDialogFor(GetCheckpoint(indexPath));
		}

		UITableViewCell GetInstructionCell(NSIndexPath indexPath)
		{
			var cell = new UITableViewCell();
			cell.TextLabel.TextAlignment = UITextAlignment.Center;
			cell.TextLabel.Lines = 0;
			cell.TextLabel.TextColor = UIColor.FromRGB(.6f, .6f, .6f);
			cell.TextLabel.Text = instructions.ElementAt(indexPath.Row).AsSentence();
			return cell;
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