using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;


namespace ClockKing
{
	public partial class CheckPointTableViewController : UITableViewController
	{
		public DataModel CheckPointData{ get; }
	
		public CheckPointTableViewController (IntPtr handle) : base (handle)
		{
			this.CheckPointData = new DataModel ();
		}
		public override void ViewDidLoad ()
		{
		
			base.ViewDidLoad ();
			this.TableView.RegisterClassForCellReuse 
				(typeof(UITableViewCell),
				CheckPointDataSource.CheckPointListCellId);

			TableView.SeparatorColor = UIColor.Gray;
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;

			this.TableView.Source = new CheckPointDataSource (this);

			this.NavigationItem.SetRightBarButtonItem(
				new UIBarButtonItem("+", UIBarButtonItemStyle.Bordered, (sender,args) => 
					this.PerformSegue("AddCheckPoint",this)
				), true);
		}

		public void DecorateCell(UITableViewCell cell, CheckPointPair checkpoints)
		{
			cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			cell.TextLabel.Text = checkpoints.firstEvent.Name;
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);

			if (segue.Identifier != "AddCheckPoint") 
			{
				var indexPath = sender as NSIndexPath;
				var checkPoints = CheckPointData.CheckPointPairs.ElementAt (indexPath.Row);
				var detailController = segue.DestinationViewController as CheckPointDetailController;
				detailController.CheckPoints = checkPoints;
			} else if (segue.Identifier == "AddCheckPoint") 
			{
				var addController = segue.DestinationViewController as AddNewCheckPointController;
				addController.Parent=this;
			}
		}
		public void AddNewCheckPoint(string title, TimeSpan target)
		{
			this.CheckPointData.AddNewCheckPoint (title, target);
			this.TableView.ReloadData ();
		}


	}

}
