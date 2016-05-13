using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;
using ClockKing.Commands;

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

			var addCommand = new AddCheckPointCommand (this);

			this.NavigationItem.SetRightBarButtonItem(addCommand.Button, true);
		}

		public void DecorateCell(UITableViewCell cell, CheckPointPair checkpoints)
		{
			
			cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			cell.TextLabel.Text = checkpoints.firstEvent.Name;
			//cell.DetailTextLabel.Text = checkpoints.firstEvent.averageObservedTime.ToString ();
		}
			
		public void AddNewCheckPoint(string title, TimeSpan target)
		{
			this.CheckPointData.AddNewCheckPoint (title, target);
			this.TableView.ReloadData ();
		}
	}
}
