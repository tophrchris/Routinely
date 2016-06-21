using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Core;
using System.Collections.Generic;
using System.Linq;
using ClockKing.Commands;
using CoreGraphics;
using iiToastNotification.Unified;
using System.Diagnostics;

namespace ClockKing
{
	
	public partial class CheckPointController : UITableViewController,IUIViewControllerPreviewingDelegate
	{
		private AppDelegate appDelegate{ get; }
		private GroupedCheckPointDataSource Data{ get; }
		public CheckPointTableCellUtilityDelegate UtilityButtonHandler{ get; }
		public CheckPointManager CheckPoints { get; }

		public NotificationManager Notifier	{ get{return this.appDelegate.Notifications; }  }
		public ClockKingOptions Options 	{ get{return this.appDelegate.Options; } }

		//move some of this into checkpointmanager?
		private DataModel CheckPointData	{ get{return this.appDelegate.CheckPointData; }}
		public CommandManager Commands		{ get{return this.appDelegate.Commands; } }
		public AddCheckPointMenuCommand AddCommand{ get; }
		private CheckPointDetailDialog currentDetailDialog {get;set;}
		private CheckpointDetailCommand Detail{ get;  }

		public CheckPointController (NSObjectFlag t):base(t){}

		public CheckPointController (IntPtr handle) : base (handle)
		{
			iiToastNotifier.Init ();

			this.appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
			this.appDelegate.Controller = this;
			this.UtilityButtonHandler = new CheckPointTableCellUtilityDelegate (this);
			this.CheckPoints = new CheckPointManager (this);
			this.CheckPoints.CheckPointDataChanged += this.RespondToChangeEvent;

			this.AddCommand = new AddCheckPointMenuCommand (this.CheckPoints);
			this.Detail = new CheckpointDetailCommand (this.CheckPoints);
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

		public void RespondToChangeEvent(object sender, CheckPointDataChangedEventArgs args)
		{
			ToastNotificationType type;

			switch (args.ActionOccurred) {
			case ActionType.Deleted:
				type = ToastNotificationType.Error;
				break;
			case ActionType.Written:
				type = ToastNotificationType.Warning;
				break;
			default:
				type = ToastNotificationType.Info;	
				break;
			}
		

			notify (args.Result.ToString(), string.Format ("{0} {1}", args.Entity, args.ActionOccurred.ToString ()), type);
			if (args.ConditionallyRefreshData)
				this.ConditionallyRefreshData (true);
			if (args.RespondToModelChanges&!args.ConditionallyRefreshData)
				this.RespondToModelChanges ();
		}

		#endregion
	
		#region data management
		/// <summary>
		/// resets the view controller to its initial state
		/// </summary>
		/// <param name="refreshData">If set to <c>true</c> refresh data.</param>
		public void ResetNavigation(bool refreshData=false){
			Debug.WriteLine("cpc reset nav");
			this.NavigationController.PopToRootViewController (true);
			this.NavigationItem.SetLeftBarButtonItem(
				new UIBarButtonItem(UIBarButtonSystemItem.Organize,(s,e)=>
				                    appDelegate.Sidebar.ToggleMenu()),true);
			                                      
			this.NavigationItem.SetRightBarButtonItem(this.AddCommand.MenuButton, true);
			this.ConditionallyRefreshData (refreshData);
			//if (!refreshData)
			//	this.RespondToModelChanges();
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
			Debug.WriteLine("cdr");
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
			Debug.WriteLine("rmc");
			this.reloadTableView ();
			appDelegate.EnsureIntegrations();
			if (currentDetailDialog != null)
				currentDetailDialog.RespondToChanges ();
		}
		private void reloadTableView()
		{
			if (this.IsViewLoaded)
			{
				this.TableView.ReloadData();
				notify("done", "table reloaded", ToastNotificationType.Info, 1);
			}
			else
				notify("oops", "view wasn't loaded", ToastNotificationType.Error);
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
			Debug.WriteLine(string.Format("{0}:{1}", title, message));
			if(this.appDelegate.Options.TracingEnabled)
				iiToastNotifier.Notify (Type, title, message, TimeSpan.FromSeconds (seconds), null, false);
		}
		#endregion
	}
}
