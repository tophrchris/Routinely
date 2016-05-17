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
		public UtilityButtonDelegate UtilityButtonHandler{ get; set; }
		public CheckpointDetailCommand Detail{ get; set; }
		public AddCheckPointCommand AddCommand{ get; set; }
		private CheckPointDataSource Data{ get;  set; }
	
		public CheckPointController (NSObjectFlag t):base(t){}

		public CheckPointController (IntPtr handle) : base (handle)
		{
			this.CheckPointData = new DataModel ();
			this.UtilityButtonHandler = new UtilityButtonDelegate (this);
			this.AddCommand = new AddCheckPointCommand (this);
			this.Detail = new CheckpointDetailCommand (this);
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

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			this.Data = new CheckPointDataSource (this);

			this.TableView.Source = this.Data;

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
