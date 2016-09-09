using Foundation;
using System;
using UIKit;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using EmojiSharp;
using Humanizer;
using ClockKing.Extensions;

namespace ClockKing
{
	public class GroupedCheckPointDataSource:UITableViewSource
	{

		private CheckPointController Controller {get;}
		private Dictionary<GroupingChoices,CheckPointGrouper> groupers { get; set; }
		private List<string> instructions;

		public void RequestRefresh() => this.groupers[ClockKingOptions.GroupingChoice].RequestForcedRefresh();

		public GroupedCheckPointDataSource (CheckPointController controller)
		{ 
			this.Controller = controller;
			this.groupers = new Dictionary<GroupingChoices, CheckPointGrouper> () 
			{
				{GroupingChoices.ByTimeOfDay,new GroupCheckPointsByTimeOfDay ()},
				{GroupingChoices.ByStatus,new GroupCheckPointsByStatus ()},
				{GroupingChoices.ByCategory,new GroupCheckPointsByCategory ()}
			};

			var right = Emoji.BLACK_RIGHTWARDS_ARROW.Unified;
			var left = Emoji.LEFTWARDS_BLACK_ARROW.Unified;
			var down = Emoji.DOWNWARDS_BLACK_ARROW.Unified;
			var gear = Emoji.All["gear"].Unified;
			var cal = Emoji.CALENDAR.Unified;

			this.instructions = new List<string>()
			{
				"To edit a goal, swipe it to the right {0}; swipe left {1} to complete it.".FormatWith(right,left),
				"pull down {0} to add a new goal".FormatWith(down),
				"press settings {0} to access settings and history {1}".FormatWith(gear,cal)
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
			return sections;
		}

		public override string TitleForHeader (UITableView tableView, nint section)
		{
			if (section == this.GroupedCheckPoints.Count())
				return "Instructions:";
			
 
			var found = this.GroupedCheckPoints.ElementAt((int)section);
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

			Controller.Refresher.EnqueueCell (cell);

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
				return this.groupers [ClockKingOptions.GroupingChoice].GroupedCheckPoints;
			}
		}
	}
}