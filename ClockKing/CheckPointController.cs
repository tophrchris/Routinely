using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;
using ClockKing.Commands;
using SWTableViewCells;

namespace ClockKing
{
	
	public partial class CheckPointController : UITableViewController
	{
		public DataModel CheckPointData{ get; }
		public UtilityButtonDelegate UtilityButtonHandler{ get; set; }
		public CheckpointDetailCommand Detail{ get; set; }
		public AddCheckPointCommand AddCommand{ get; set; }
	
		public CheckPointController (IntPtr handle) : base (handle)
		{
			this.CheckPointData = new DataModel ();
			this.UtilityButtonHandler = new UtilityButtonDelegate (this);
			this.AddCommand = new AddCheckPointCommand (this);
			this.Detail = new CheckpointDetailCommand (this);

		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.TableView.Source = new CheckPointDataSource (this);

			this.NavigationItem.SetRightBarButtonItem(AddCommand.Button, true);
		}
			
		public void AddNewCheckPoint(string title, TimeSpan target)
		{
			this.CheckPointData.AddNewCheckPoint (title, target);
			this.ReloadData ();
		}

		public void ReloadData()
		{
			this.TableView.ReloadData ();
		}
	}
}
