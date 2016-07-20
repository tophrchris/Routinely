using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using ClockKing.Core;

namespace ClockKing
{
	public class BlankCheckPointDataSource:UITableViewSource
	{
		List<string> instructions;
		public BlankCheckPointDataSource()
		{
			instructions = new List<string>
				{
				"Welcome to Routinely!",
				"Routinely helps you establish and track the habits that make up your daily routine.",
				"get started by adding a daily goal:",
				"you can press the + button, or pull down" };
		}

		public override nfloat GetHeightForRow(UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			return 100;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			return instructions.Count;
		}
		public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
		{
			var cell = new UITableViewCell();
			cell.TextLabel.TextAlignment = UITextAlignment.Center;
			cell.TextLabel.Lines = 0;
			cell.TextLabel.Text = instructions.ElementAt(indexPath.Row).AsSentence();
			return cell;
		}
	}
}

