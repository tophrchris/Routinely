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
		public DataModel CheckPointData{ get; set;}
		public CommandManager Commands{ get; }
		public CheckpointCommandDelegate UtilityButtonHandler{ get; set; }
		public CheckpointDetailCommand Detail{ get; set; }
		public AddCheckPointCommand AddCommand{ get; set; }
		public NotificationManager Notifier{ get; set; }
		private CheckPointDataSource Data{ get;  set; }
		private AppDelegate appDelegate{ get; set; }

		public CheckPointController (NSObjectFlag t):base(t){}

		public CheckPointController (IntPtr handle) : base (handle)
		{
			this.appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
			this.appDelegate.Controller = this;
			this.Notifier = appDelegate.Notifications;

			ConditionallyRefreshData ();

			this.Commands = appDelegate.Commands;

			this.UtilityButtonHandler = new CheckpointCommandDelegate (this);
			this.AddCommand = new AddCheckPointCommand (this);
			this.Detail = new CheckpointDetailCommand (this);
			this.NavigationItem.SetRightBarButtonItem(this.AddCommand.Button, true);
			this.Data = new CheckPointDataSource (this);

		}
			
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.TableView.Source = this.Data;
		}

		public override void ViewDidAppear (bool animated)
		{			
			base.ViewDidAppear (animated);
			this.ConditionallyRefreshData ();
		}
			
			


		public CheckPoint AddNewCheckPoint(string title, TimeSpan target,string emoji)
		{
			if (string.IsNullOrEmpty (emoji))
				emoji = title.Substring (0, 2);
			
			var created = this.CheckPointData.AddNewCheckPoint (title, target,emoji);
			if(created!=null)
				this.RespondToModelChanges ();
			
			return created;
		}

		public bool RemoveCheckpoint(CheckPoint toDelete)
		{
			var deleted =  this.CheckPointData.RemoveCheckPoint (toDelete);

			if (deleted) 
				this.RespondToModelChanges ();
			
			return deleted;
		}

		public Occurrence AddOccurrenceToCheckPoint(string checkPointName, int mins)
		{
			var found = this.CheckPointData
							.CheckPointPairs
							.Select (cpp => cpp.firstEvent)
				.FirstOrDefault (cp => cp.Name == checkPointName);
			return this.AddOccurrenceToCheckPoint (found, mins);


		}

		public Occurrence AddOccurrenceToCheckPoint(CheckPoint checkPoint,int mins)
		{
			var o = checkPoint.CreateOccurrence(DateTime.Now.ToLocalTime().AddMinutes(mins));
			checkPoint.AddOccurrence (o);
			this.CheckPointData.SaveOccurrence (o);
			this.RespondToModelChanges ();
			return o;
		}



		public bool ConditionallyRefreshData()
		{
			if (appDelegate.RequiresDataRefresh) 
			{
				this.CheckPointData = new DataModel ();
				appDelegate.RequiresDataRefresh = false;
				this.RespondToModelChanges ();
				return true;
			}
			return false;
		}

		public void RespondToModelChanges()
		{
			if(this.IsViewLoaded)
				this.TableView.ReloadData ();
			this.Notifier.EnsureNotifications (this.CheckPointData);
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
