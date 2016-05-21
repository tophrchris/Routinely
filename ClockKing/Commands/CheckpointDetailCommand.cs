using System;
using Foundation;
using UIKit;
using ClockKing.Model;
using MonoTouch.Dialog;
using System.Linq;
using System.Collections.Generic;

namespace ClockKing
{
	public class CheckpointDetailCommand
	{

		protected UIViewController Controller{ get; set;}

		public CheckpointDetailCommand (UIViewController controller)
		{
			this.Controller = controller;

		}

		public UIViewController GetDetailDialog(CheckPoint Data)
		{
			var checkpoint = Data;
			var distinctTimes = checkpoint.Occurrences.Select (o => o.Time).Distinct();

			var root = new RootElement (string.Format ("details for {0}", checkpoint.Name));
			var timingSection = new Section ("occurences");
			root.Add (timingSection);

			timingSection.Add (new StringElement ("count", checkpoint.Occurrences.Count().ToString()));
			timingSection.Add (new StringElement("average", (DateTime.Now.Date+ checkpoint.averageObservedTime).ToString("t")));

			if (checkpoint.Occurrences.Any ()) {
				timingSection.Add (new StringElement ("stdev", checkpoint.Occurrences.Average (o => checkpoint.averageObservedTime.Minutes - o.Time.Minutes).ToString ()));
				timingSection.Add (new StringElement ("earliest", distinctTimes.OrderBy (o => o.TotalMinutes).First ().ToString ()));
				timingSection.Add (new StringElement ("latest", distinctTimes.OrderByDescending (o => o.TotalMinutes).First ().ToString ()));

				var ds = new Section ("details");
				var detailRoot = new RootElement ("details");
				var detailsSection = new Section ();
				ds.Add (root);
				detailRoot.Add (detailsSection);
				detailsSection.AddAll (
					checkpoint.Occurrences.Select (o => new StringElement (o.timeStamp.Date.ToString (), o.Time.ToString ())));

				root.Add (detailRoot);
			}

			return new CheckPointDetailViewController (this.Controller,Data, root);
		}

		public void ShowDetailDialog(CheckPoint Data)
		{
			var mtd = GetDetailDialog (Data);

			ShowDetailDialog (mtd);

		}
		public void ShowDetailDialog(UIViewController dialog)
		{
			this.Controller.NavigationController.PushViewController (dialog,true);

			dialog.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Done,
				(s,e)=>this.Controller.NavigationController.PopViewController(true)
			), true);
		}
	}

	public class CheckPointDetailViewController:DialogViewController
	{
		private List<UIPreviewAction> actions { get; set;}

		public CheckPointDetailViewController(UIViewController Parent,CheckPoint toDetail,RootElement root):base(root)
		{
			var parent = Parent as CheckPointController;
		
			var executor = new Action<Command>((ub)=>
				{
					if(ub.ExecuteFor(parent,toDetail))
						parent.ReloadData();
				});

			this.actions = parent.Commands.GetPreviewActionsForCheckpoint (toDetail, executor).ToList();
		}

		public override IUIPreviewActionItem[] PreviewActionItems 
		{
			get {
				return this.actions.ToArray();
			}
		}
	}
}