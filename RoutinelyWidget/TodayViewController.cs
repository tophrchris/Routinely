using System;
using System.Diagnostics;
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

			//var e = UIVibrancyEffect.CreateSecondaryVibrancyEffectForNotificationCenter();

			//ExtensionContext.SetWidgetLargestAvailableDisplayMode(NCWidgetDisplayMode.Expanded);
			this.observer = NSNotificationCenter.DefaultCenter.
				AddObserver((NSString)"NSUserDefaultsDidChangeNotification",
							(NSNotification obj) => update());


			var pp = new AppGroupPathProvider(".json");
			var pv = new JSONDataProvider(pp);
			this.Model = new DataModel(pv, true);

			this.TableView.Source = new GoalWidgetDataSource(this, Model);

			var goals = (float)this.TableView.Source.RowsInSection(this.TableView, 0);
			this.PreferredContentSize = new CGSize(0, ((CheckPointTableCell.Height - 70f) * goals) * 1.1f);

			this.TableView.RegisterClassForCellReuse(typeof(CheckPointTableCell), CheckPointTableCell.Key);

			this.TableView.SeparatorEffect = UIVibrancyEffect.CreateSecondaryVibrancyEffectForNotificationCenter();
			this.TableView.SeparatorColor = UIColor.FromWhiteAlpha(1.0f, .5f);

			this.update(false);

		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			update();
			base.TouchesEnded(touches, evt);
		}

		public override void ViewDidUnload()
		{
			base.ViewDidUnload();
			if (observer != null)
				NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
			
			observer = null;

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

		public void update(bool refreshData=true)
		{
				if (refreshData)
					this.Model.RefreshData(refreshData);

				this.TableView.ReloadData();
		}

	}
	public class GoalWidgetDataSource : UITableViewSource
	{
		private DataModel Data { get; set; }
		private TodayViewController Controller { get; set; }
		private CheckPoint prev
		{
			get
			{	var last = Data.LastCheckpoint;
				if (last != null)
					return last;
				var mr = Data.MostRecentCompletedCheckpoint;
				if (mr != null)
					return mr;
				return null;
			}
		}
		private bool hasPrev
		{
			get
			{
				return prev != null;
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

		public override UIView GetViewForFooter(UITableView tableView, nint section)
		{
			var footer = new UILabel();

			footer.Font = UIFont.FromName("AvenirNextCondensed-UltraLight", 8f);
			footer.TextColor = UIColor.DarkGray;
			footer.TextAlignment = UITextAlignment.Right;
			footer.Text=string.Format("Last Updated: {0}", DateTime.Now.ToString("G"));
			return footer;
		}

		public override nfloat GetHeightForFooter(UITableView tableView, nint section)
		{
			return 12f;
		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			var cell = GetCell(tableView, indexPath) as CheckPointTableCell;
			var url = new NSUrl("Routinely://" + cell.CheckPoint.UniqueIdentifier);
			cell.TitleLabel.Text += "!";
			Controller.ExtensionContext.OpenUrl(url, (c) => this.Controller.update() );
		}

		public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
		{
			return CheckPointTableCell.Height-70f;
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
				goal = prev;
			else
				goal = Data.NextCheckpoint;

			return goal;
		}
	}
}

