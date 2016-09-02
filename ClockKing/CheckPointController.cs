using Foundation;
using System;
using UIKit;
using ClockKing.Core;
using ClockKing.Extensions;
using System.Linq;
using ClockKing.Commands;
using CoreGraphics;
using iiToastNotification.Unified;
using System.Diagnostics;



namespace ClockKing
{

	public partial class CheckPointController : UITableViewController,IUIViewControllerPreviewingDelegate
	{
		private AppDelegate App{ get { return UIApplication.SharedApplication.Delegate as AppDelegate; } }
		private GroupedCheckPointDataSource Data{ get; }
		public CheckPointTableCellUtilityDelegate UtilityButtonHandler{ get; }
		public CheckPointManager CheckPoints { get; }

		public NotificationManager Notifier	{ get{return this.App.Notifications; }  }

		//move some of this into checkpointmanager?
		private DataModel CheckPointData	{ get{return this.App.CheckPointData; }}
		public CommandManager Commands		{ get{return this.App.Commands; } }
		public AddCheckPointMenuCommand AddCommand{ get; }
		private CheckPointDetailDialog currentDetailDialog {get;set;}
		private CheckpointDetailCommand Detail{ get;  }
		public TableCellRefresher Refresher { get; set; }

		public CheckPointController (NSObjectFlag t):base(t){}

		public CheckPointController (IntPtr handle) : base (handle)
		{
			iiToastNotifier.Init ();

			this.App.Controller = this;
			this.UtilityButtonHandler = new CheckPointTableCellUtilityDelegate (this);
			this.CheckPoints = new CheckPointManager (this);
			this.CheckPoints.CheckPointDataChanged += this.RespondToChangeEvent;

			this.AddCommand = new AddCheckPointMenuCommand (this.CheckPoints);
			this.Detail = new CheckpointDetailCommand (this.CheckPoints);
			this.Data = new GroupedCheckPointDataSource (this);

			this.Refresher = new TableCellRefresher((a) => this.InvokeOnMainThread(a));

			this.ResetNavigation ();
		}

		#region app lifecycle and maintenance
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.reloadTableView();

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
			this.App.LogActivity("Goal Listing");
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

			var ud = new NSUserDefaults(AppGroupPathProvider.SuiteName, NSUserDefaultsType.SuiteName);
			ud.SetValueForKey(DateTime.Now.ToNSDate(), new NSString("MRU"));
			ud.Synchronize();

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
			Debug.WriteLine("checkpoint controller: reset nav");
			this.NavigationController.PopToRootViewController (true);
			this.NavigationItem.SetLeftBarButtonItem(
				new UIBarButtonItem(EmojiSharp.Emoji.All["gear"].Unified,UIBarButtonItemStyle.Plain,(s,e)=>
				                    App.Sidebar.ToggleMenu()),true);
			                                      
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
			var updated= this.ConditionallyRefreshData (App.RequiresDataRefresh);
			if (updated)
				App.RequiresDataRefresh = false;
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
				this.App.CheckPointData.RefreshData ();
				notify ("done", "data refreshed", ToastNotificationType.Info, 1);
				dataUpdated=true;
				this.RespondToModelChanges ();
			}
			if(this.IsViewLoaded)
				while (App.LaunchActions.Any ())
					App.LaunchActions.Dequeue ().Invoke (this);
			
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
			App.EnsureIntegrations();
			if (currentDetailDialog != null)
				currentDetailDialog.RespondToChanges ();
		}
		private void reloadTableView()
		{
			EnsureDataSource();

			if (this.IsViewLoaded)
			{
				Refresher.Restart ();
				this.TableView.ReloadData();


				notify("done", "table reloaded", ToastNotificationType.Info, 1);
			}
			else
				notify("oops", "view wasn't loaded", ToastNotificationType.Error);
		}

		private void EnsureDataSource()
		{
			if (this.TableView.Source == null)
			{
				if (this.CheckPointData.checkPoints.Any())
					this.TableView.Source = this.Data;
				else
					this.TableView.Source = new BlankCheckPointDataSource();
			}
			else
			{
				if (this.TableView.Source is BlankCheckPointDataSource)
					if (this.CheckPointData.checkPoints.Any())
						this.TableView.Source = this.Data;
				if (!this.CheckPointData.checkPoints.Any())
					if (!(this.TableView.Source is BlankCheckPointDataSource))
						this.TableView.Source = new BlankCheckPointDataSource();
			}
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
			if(ClockKingOptions.TracingEnabled)
				iiToastNotifier.Notify (Type, title, message, TimeSpan.FromSeconds (seconds), null, false);
		}
		#endregion
	}
}
