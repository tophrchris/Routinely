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
using iiToastNotification.Unified;

namespace ClockKing
{
	
	public partial class CheckPointController : UITableViewController,IUIViewControllerPreviewingDelegate
	{
		private AppDelegate appDelegate{ get; }
		private ShowSettingsMenuCommand showNotifications{ get; }
		private CheckpointDetailCommand Detail{ get;  }
		private GroupedCheckPointDataSource Data{ get; }
		public CheckpointCommandDelegate UtilityButtonHandler{ get; }
		public AddCheckPointMenuCommand AddCommand{ get; }
		private CheckPointDetailDialog currentDetailDialog {get;set;}

		private DataModel CheckPointData	{ get{return this.appDelegate.CheckPointData; }}
		public NotificationManager Notifier	{ get{return this.appDelegate.Notifications; }  }
		public CommandManager Commands		{ get{return this.appDelegate.Commands; } }
		public ClockKingOptions Options 	{ get{return this.appDelegate.Options; } }

		public CheckPointController (NSObjectFlag t):base(t){}

		public CheckPointController (IntPtr handle) : base (handle)
		{
			iiToastNotifier.Init ();

			this.appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
			this.appDelegate.Controller = this;

			this.UtilityButtonHandler = new CheckpointCommandDelegate (this);
			this.AddCommand = new AddCheckPointMenuCommand (this);
			this.showNotifications = new ShowSettingsMenuCommand (this);
			this.Detail = new CheckpointDetailCommand (this);
			this.Data = new GroupedCheckPointDataSource (this,CheckPointData);

			this.ResetNavigation ();
		}
			
		#region app lifecycle and maintenance
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

		public void ResetNotifications()
		{
			this.Notifier.EnsureNotifications (this.CheckPointData, true);
		}
		#endregion

			
		public void ShowDetailDialogFor(CheckPoint checkpoint)
		{
			this.currentDetailDialog =  this.Detail.ShowDetailDialog (checkpoint);
		}	


		#region model access
		public bool CheckPointExists(string name)
		{
			return this.CheckPointData.checkPoints.ContainsKey (name);
		}


		public CheckPoint AddNewCheckPoint(string title, TimeSpan target,string emoji)
		{
			if (string.IsNullOrEmpty (emoji))
				emoji = title.Substring (0, 2);

			var created = this.CheckPointData.AddNewCheckPoint (title, target,emoji);
			if (created != null) 
			{
				notify("Success", "New Goal Added");
				this.RespondToModelChanges ();
			}
			return created;
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
			notify("Success", "Occurrence saved");
			this.RespondToModelChanges ();
			return o;
		}
			
		public void RewriteOccurrences()
		{
			notify ("success", "occurrences rewritten", ToastNotificationType.Warning);
			this.CheckPointData.SaveOccurrences ();
			this.ConditionallyRefreshData ();
		}

		public bool ResaveCheckpoints()
		{
			var saved = this.CheckPointData.SaveCheckPoints ();
			if (saved) 
			{
				notify("Success", "Goals saved",ToastNotificationType.Warning);
				this.RespondToModelChanges();
			}
			return saved;
		}
			
		public bool RemoveCheckpoint(CheckPoint toDelete)
		{
			var deleted =  this.CheckPointData.RemoveCheckPoint (toDelete);

			if (deleted) { 
				notify("Success", "Goal Removed");
				this.ResetNavigation();
				this.RespondToModelChanges ();
			}
			else
				notify("failure", "Goal not removed?",ToastNotificationType.Error);
			
			return deleted;
		}

		#endregion
	
		#region data management
		/// <summary>
		/// resets the view controller to its initial state
		/// </summary>
		/// <param name="refreshData">If set to <c>true</c> refresh data.</param>
		public void ResetNavigation(bool refreshData=false){
			this.NavigationController.PopToRootViewController (true);
			this.NavigationItem.SetLeftBarButtonItem(this.showNotifications.MenuCommand,true);
			this.NavigationItem.SetRightBarButtonItem(this.AddCommand.MenuButton, true);
			this.ConditionallyRefreshData (refreshData);
		}
			
		/// <summary>
		/// this should be called if there is a chance that the data was updated without
		/// user interaction.
		/// </summary>
		public bool ConditionallyRefreshData()
		{
			var updated= this.ConditionallyRefreshData (appDelegate.RequiresDataRefresh);
			if (updated)
				appDelegate.RequiresDataRefresh = false;
			return updated;
		}
			
		/// <summary>
		/// this should be called if there is a chance that the data was updated without
		/// user interaction.
		/// </summary>
		/// <returns><c>true</c>, if data was refreshed, <c>false</c> otherwise.</returns>
		/// <param name="condition"></param>
		public bool ConditionallyRefreshData(bool condition)
		{
			var dataUpdated = false;
			if (condition) 
			{
				this.appDelegate.CheckPointData.RefreshData ();
				notify ("done", "data refreshed", ToastNotificationType.Info, 1);
				dataUpdated=true;
				this.RespondToModelChanges ();
			}
			if(this.IsViewLoaded)
				while (appDelegate.LaunchActions.Any ())
					appDelegate.LaunchActions.Dequeue ().Invoke (this);
			
			return dataUpdated;
		}

		/// <summary>
		/// this should be called if a model change has occurred without 
		/// also interracting with the controller before navigating
		/// </summary>
		public void RespondToModelChanges()
		{
			this.reloadTableView ();
			notify ("done", "table reloaded", ToastNotificationType.Info, 1);
			this.Notifier.EnsureNotifications (this.CheckPointData);
			ShortcutManager.CreateShortcutItems (UIApplication.SharedApplication, this.CheckPointData);
			if (currentDetailDialog != null)
				currentDetailDialog.RespondToChanges ();
		}
		private void reloadTableView()
		{
			if (this.IsViewLoaded)
				this.TableView.ReloadData ();
		}
		#endregion

		#region pop/peek

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
			
			this.Detail.ShowDetailDialog (viewControllerToCommit as CheckPointDetailDialog);
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
		#endregion

		#region tracing aids
		public void notify(string title, string message,
			ToastNotificationType Type = ToastNotificationType.Success,
			int seconds=1)
		{
			if(this.appDelegate.Options.TracingEnabled)
				iiToastNotifier.Notify (Type, title, message, TimeSpan.FromSeconds (seconds), null, false);
		}
		#endregion
	}
}
