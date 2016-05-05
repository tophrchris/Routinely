using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;
using ClockKing.Model;
using System.Collections.Generic;
using System.Linq;


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

			TableView.SeparatorColor = UIColor.Blue;
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.DoubleLineEtched;

			// blur effect
			TableView.SeparatorEffect = 
				UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);

			//vibrancy effect
			var effect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Light);
			TableView.SeparatorEffect = UIVibrancyEffect.FromBlurEffect(effect);

			this.TableView.Source = new CheckPointDataSource (this);

		}

		public void DecorateCell(UITableViewCell cell, CheckPointPair checkpoints)
		{
			cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
			cell.TextLabel.Text = checkpoints.firstEvent.Name;
			//cell.DetailTextLabel.Text = checkpoints.firstEvent.averageObservedTime.ToString();
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			base.PrepareForSegue (segue, sender);
		}

	}

}
