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

			this.TableView.Source = new CheckPointDataSource (this);

			var addCommand = new AddCheckPointCommand (this);

			this.NavigationItem.SetRightBarButtonItem(addCommand.Button, true);
		}
			
		public void AddNewCheckPoint(string title, TimeSpan target)
		{
			this.CheckPointData.AddNewCheckPoint (title, target);
			this.TableView.ReloadData ();
		}
	}
}
