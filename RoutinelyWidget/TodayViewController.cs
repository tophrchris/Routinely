﻿using System;

using NotificationCenter;
using Foundation;
using UIKit;
using CoreGraphics;
using Cirrious.FluentLayouts.Touch;
using ClockKing.Core;
using ClockKing;
using System.Linq;

namespace RoutinelyWidget
{
	public partial class TodayViewController : UITableViewController, INCWidgetProviding
	{
		private DataModel Model { get; set; }
		private NSObject observer { get; set; }
		protected TodayViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void DidReceiveMemoryWarning()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			this.PreferredContentSize = new CGSize(100f, 100f);

			var pp = new AppGroupPathProvider(".json");
			var pv = new JSONDataProvider(pp);
			this.Model = new DataModel(pv, true);
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.TableView.RegisterClassForCellReuse(typeof(CheckPointTableCell), CheckPointTableCell.Key);
			this.TableView.Source = new GoalWidgetDataSource(this,Model);

			this.observer = NSNotificationCenter.DefaultCenter.
				AddObserver((NSString)"NSUserDefaultsDidChangeNotification",

				            (NSNotification obj) => update());

		}

		public override void ViewDidUnload()
		{
			base.ViewDidUnload();
			if (observer != null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
				observer = null;
			}
		}

		[Export("widgetPerformUpdateWithCompletionHandler:")]
		public void WidgetPerformUpdate(Action<NCUpdateResult> completionHandler)
		{
			update();
		
			// Perform any setup necessary in order to update the view.
			// If an error is encoutered, use NCUpdateResultFailed
			// If there's no update required, use NCUpdateResultNoData
			// If there's an update, use NCUpdateResultNewData
			//update();

			completionHandler(NCUpdateResult.NewData);
		}
		private void update()
		{
			this.Model.RefreshData(true);
			this.TableView.ReloadData();
		}

	}
	public class GoalWidgetDataSource : UITableViewSource
	{
		private DataModel Data { get; set; }
		private TodayViewController Controller { get; set; }
		private bool hasPrev
		{
			get
			{
				return Data.LastCheckpoint != null;
			}
		}
		private bool hasNext
		{
			get
			{
				return Data.NextCheckpoint != null;
			}
		}
		public GoalWidgetDataSource(TodayViewController controller, DataModel data)
		{
			this.Controller = controller;
			this.Data = data;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = GetCell(tableView, indexPath) as CheckPointTableCell;
			var url = new NSUrl("Routinely://" + cell.CheckPoint.UniqueIdentifier);
			cell.TitleLabel.Text += "!";
			Controller.ExtensionContext.OpenUrl(url, null);
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			return CheckPointTableCell.Height-60f;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
			var rows = 0;
			if (hasPrev)
				rows++;
			if (hasNext)
				rows++;
			return rows;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{
			return CheckPointTableCellFactory(tableView, indexPath); ;
		}

		private UITableViewCell CheckPointTableCellFactory(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = new CheckPointTableCell((float)Controller.View.Frame.Width,CheckPointTableCell.DisplayModes.Widget);

			CheckPoint goal = GetGoal(tableView,indexPath);

			cell.RenderCheckpoint(goal);

			return cell;
		}

		private CheckPoint GetGoal(UITableView tableView, NSIndexPath indexPath)
		{
			int rows = (int)RowsInSection(tableView, indexPath.Section)-1;
			int row = indexPath.Row;
			CheckPoint goal = null;
			if (row < rows)
				goal = Data.LastCheckpoint;
			else
				goal = Data.NextCheckpoint;

			return goal;
		}
	}
}

