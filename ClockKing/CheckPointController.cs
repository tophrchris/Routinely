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
using CoreGraphics;

namespace ClockKing
{
	
	public partial class CheckPointController : UITableViewController,IUIViewControllerPreviewingDelegate
	{
		public DataModel CheckPointData{ get; }
		public CommandManager Commands{ get; }
		public CheckpointCommandDelegate UtilityButtonHandler{ get; set; }
		public CheckpointDetailCommand Detail{ get; set; }
		public AddCheckPointCommand AddCommand{ get; set; }
		public NotificationManager Notifier{ get; set; }
		private CheckPointDataSource Data{ get;  set; }

		public CheckPointController (NSObjectFlag t):base(t){}

		public CheckPointController (IntPtr handle) : base (handle)
		{
			var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
			this.CheckPointData = new DataModel ();
			this.Commands = appDelegate.Commands;
			this.Notifier = appDelegate.Notifications;
			this.UtilityButtonHandler = new CheckpointCommandDelegate (this);
			this.AddCommand = new AddCheckPointCommand (this);
			this.Detail = new CheckpointDetailCommand (this);

			this.Notifier.EnsureNotifications (this.CheckPointData);
		}
			
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.Data = new CheckPointDataSource (this);
			this.TableView.Source = this.Data;
			this.NavigationItem.SetRightBarButtonItem(this.AddCommand.Button, true);
		}
			



		public CheckPoint AddNewCheckPoint(string title, TimeSpan target,string emoji)
		{
			if (string.IsNullOrEmpty (emoji))
				emoji = title.Substring (0, 2);
			
			var created = this.CheckPointData.AddNewCheckPoint (title, target,emoji);
			this.Notifier.EnsureNotifications (this.CheckPointData);
			this.ReloadData ();
			return created;
		}

		public bool RemoveCheckpoint(CheckPoint toDelete)
		{
			var deleted =  this.CheckPointData.RemoveCheckPoint (toDelete);

			if (deleted) 
			{
				this.Notifier.EnsureNotifications (this.CheckPointData);
				this.ReloadData ();
			}
			return deleted;
		}

		public void ReloadData()
		{
			this.TableView.ReloadData ();
		}




		public override void TraitCollectionDidChange (UITraitCollection previousTraitCollection)
		{
			base.TraitCollectionDidChange(previousTraitCollection);

			try{
				if (TraitCollection.ForceTouchCapability == UIForceTouchCapability.Available) {
					RegisterForPreviewingWithDelegate(this, this.View);
				}
			}catch{
			}
		}
			 
		public  void CommitViewController (IUIViewControllerPreviewing previewingContext, UIViewController viewControllerToCommit)
		{
			this.Detail.ShowDetailDialog (viewControllerToCommit);
		}

		public  UIViewController GetViewControllerForPreview (IUIViewControllerPreviewing previewingContext, CoreGraphics.CGPoint location)
		{

			var indexPath = this.TableView.IndexPathForRowAtPoint (location);
			var cell = this.TableView.CellAt (indexPath);
			var item = this.Data.GetCheckpoint(indexPath);

			var previewer= Detail.GetDetailDialog (item);

			previewingContext.SourceRect = cell.Frame;
			previewer.PreferredContentSize = new CGSize (0, 0);

			return previewer;
		}
	}
}
