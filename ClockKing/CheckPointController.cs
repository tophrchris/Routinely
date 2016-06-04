using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Core;
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
		private DataModel CheckPointData{ get; set;}
		private GroupedCheckPointDataSource Data{ get; }
		private AppDelegate appDelegate{ get; }
		public AddCheckPointMenuCommand AddCommand{ get; }
		private ShowSettingsMenuCommand showNotifications{ get; }
		private CheckpointDetailCommand Detail{ get;  }

		public CommandManager Commands{ get; }
		public CheckpointCommandDelegate UtilityButtonHandler{ get; }
		public NotificationManager Notifier{ get;  }
		public ClockKingOptions Options { get { return this.appDelegate.Options; } }

		public CheckPointController (NSObjectFlag t):base(t){}

		public CheckPointController (IntPtr handle) : base (handle)
		{
			this.appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
			this.appDelegate.Controller = this;
			this.Notifier = appDelegate.Notifications;
			this.CheckPointData = appDelegate.CheckPointData;
			this.Commands = appDelegate.Commands;
			this.UtilityButtonHandler = new CheckpointCommandDelegate (this);
			this.AddCommand = new AddCheckPointMenuCommand (this);
			this.showNotifications = new ShowSettingsMenuCommand (this);
			this.Detail = new CheckpointDetailCommand (this);
			this.Data = new GroupedCheckPointDataSource (this,CheckPointData);

			this.ResetNavigation ();
		}

		public void ResetNavigation(){
			this.NavigationController.PopToRootViewController (true);
			this.NavigationItem.SetLeftBarButtonItem(this.showNotifications.MenuCommand,true);
			this.NavigationItem.SetRightBarButtonItem(this.AddCommand.MenuButton, true);
			this.ConditionallyRefreshData ();
		}

			
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.TableView.Source = this.Data;

			this.RefreshControl.ValueChanged += (o, e) => {
				this.RefreshControl.EndRefreshing();
				this.AddCommand.ShowDialog();
			};
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			this.reloadTableView ();
		}

		public override void ViewDidAppear (bool animated)
		{			
			base.ViewDidAppear (animated);
			this.ConditionallyRefreshData ();
		}
			
		public void ShowDetailDialogFor(CheckPoint checkpoint)
		{
			this.Detail.ShowDetailDialog (checkpoint);
		}	

		public void ResetNotifications()
		{
			this.Notifier.EnsureNotifications (this.CheckPointData, true);
		}

		public void RewriteOccurrences()
		{
			this.CheckPointData.SaveOccurrences ();
		}

		public bool CheckPointExists(string name)
		{
			return this.CheckPointData.checkPoints.ContainsKey (name);
		}

		public bool ResaveCheckpoints()
		{
			var saved = this.CheckPointData.SaveCheckPoints ();
			if (saved)
				this.RespondToModelChanges ();
			return saved;

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
			var found = this.CheckPointData.checkPoints [checkPointName];
			return this.AddOccurrenceToCheckPoint (found, mins);
		}

		public Occurrence AddOccurrenceToCheckPoint(CheckPoint checkPoint,int mins)
		{
			return AddOccurrenceToCheckPoint(checkPoint, DateTime.Now.ToLocalTime ().AddMinutes (mins));
		}

		public Occurrence AddOccurrenceToCheckPoint(CheckPoint checkPoint,DateTime when)
		{
			var o = checkPoint.CreateOccurrence(when.ToLocalTime());
			checkPoint.AddOccurrence (o);
			this.CheckPointData.SaveOccurrence (o);
			this.RespondToModelChanges ();
			return o;
		}


		public bool ConditionallyRefreshData()
		{
			var updated= this.ConditionallyRefreshData (appDelegate.RequiresDataRefresh);
			if (updated)
				appDelegate.RequiresDataRefresh = false;
			return updated;
		}

		public bool ConditionallyRefreshData(bool condition)
		{

			var dataUpdated = false;
			if (condition) 
			{
				this.appDelegate.CheckPointData = new DataModel ();
				this.RespondToModelChanges ();
				dataUpdated=true;
			}
			if(this.IsViewLoaded)
				while (appDelegate.LaunchActions.Any ())
					appDelegate.LaunchActions.Dequeue ().Invoke (this);
			
			return dataUpdated;
		}

		public void RespondToModelChanges()
		{
			this.reloadTableView ();
				this.Notifier.EnsureNotifications (this.CheckPointData);
				ShortcutManager.CreateShortcutItems (UIApplication.SharedApplication, this.CheckPointData);
		}
		private void reloadTableView()
		{
			if (this.IsViewLoaded)
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
