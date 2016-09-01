using System;

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

			/*
			this.TodayMessage = new UILabel();
			this.TodayMessage.TextAlignment = UITextAlignment.Center;
			this.View.BackgroundColor = UIColor.Green;
			TodayMessage.TextColor = UIColor.White;
			TodayMessage.Lines = 0;
			TodayMessage.LineBreakMode = UILineBreakMode.WordWrap;
			this.View.AddSubview(TodayMessage);
			View.SubviewsDoNotTranslateAutoresizingMaskIntoConstraints();
			View.AddConstraints(
				TodayMessage.AtLeftOf(View),
				TodayMessage.AtTopOf(View),
				TodayMessage.AtBottomOf(View)
				) ;
			View.AddConstraints(TodayMessage.FullHeightOf(View));
			View.LayoutIfNeeded();
			*/
			//TodayMessage.Text = "starting:" + Environment.NewLine;
			var pp = new AppGroupPathProvider(".json");
			//TodayMessage.Text += pp.AppGroupPath + Environment.NewLine;
			var pv = new JSONDataProvider(pp);
			//TodayMessage.Text += pv.ReadCheckPoints().Count() + Environment.NewLine;

			this.Model = new DataModel(pv, true);
			this.TableView.Source = new GoalWidgetDataSource(this, Model);




		

			// Do any additional setup after loading the view.
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			//update(UIColor.Purple);
			if (this.View.BackgroundColor == UIColor.Green)
				this.View.BackgroundColor = UIColor.Orange;
			else
				this.View.BackgroundColor = UIColor.Green;
		}

		[Export("widgetPerformUpdateWithCompletionHandler:")]
		public void WidgetPerformUpdate(Action<NCUpdateResult> completionHandler)
		{
			// Perform any setup necessary in order to update the view.

			// If an error is encoutered, use NCUpdateResultFailed
			// If there's no update required, use NCUpdateResultNoData
			// If there's an update, use NCUpdateResultNewData
			//update();

			completionHandler(NCUpdateResult.NewData);
		}

		/*private void update(UIColor color)
		{
			this.View.BackgroundColor = color;
			TodayMessage.Text = string.Empty;

			try
			{
				
				TodayMessage.Text += this.Model.checkPoints.Count + Environment.NewLine;

				if (Model.LastCheckpoint != null)
					TodayMessage.Text = Model.LastCheckpoint.Name + Environment.NewLine;

				if (Model.NextCheckpoint != null)
					TodayMessage.Text += Model.NextCheckpoint.Name;

			}
			catch (Exception e)
			{
				TodayMessage.Text += Environment.NewLine+ "whoops" + Environment.NewLine+ e.Message;

			}
		}
		*/
	}
	public class GoalWidgetDataSource : UITableViewSource
	{
		private TodayViewController Controller { get; set; }
		private DataModel Data { get; set; }
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
			var cell = new UITableViewCell(UITableViewCellStyle.Subtitle, "goalcell");
			int rows = (int)RowsInSection(this.Controller.TableView, indexPath.Section);
			int row = indexPath.Row;
			CheckPoint goal = null;
			if (row < rows)
				goal = Data.LastCheckpoint;
			else
				goal = Data.NextCheckpoint;

			cell.TextLabel.Text = goal.Name;

			return cell;

				
		}
	}
}

