using System;
using Foundation;
using UIKit;
using ClockKing.Model;
using MonoTouch.Dialog;
using System.Linq;
using System.Collections.Generic;
using Humanizer;

namespace ClockKing
{
	public class CheckpointDetailCommand
	{

		protected CheckPointController Controller{ get; set;}

		public CheckpointDetailCommand (CheckPointController controller)
		{
			this.Controller = controller;

		}

		public UIViewController GetDetailDialog(CheckPoint Data)
		{
			var checkpoint = Data;
			var distinctTimes = checkpoint.Occurrences.Select (o => o.Time).Distinct();

			var root = new RootElement (string.Format ("details for {0}", checkpoint.Name));
			var timingSection = new Section ("Stats:");
			root.Add (timingSection);

			timingSection.Add (new StringElement ("count",
				checkpoint.Occurrences.Count().ToString()));
			timingSection.Add (new StringElement("average", 
				(DateTime.Now.Date+ checkpoint.averageObservedTime).ToString("t")));
			timingSection.Add (new StringElement("next",
				"in " + checkpoint.UntilNextTargetTime.Humanize(2)));

			if (checkpoint.Occurrences.Any ()) {
				//timingSection.Add (new StringElement ("stdev", checkpoint.Occurrences.Average (o => checkpoint.averageObservedTime.Minutes - o.Time.Minutes).ToString ()));
				timingSection.Add (new StringElement ("earliest",
					(DateTime.Today+ distinctTimes.OrderBy (o => o.TotalMinutes).First ()).ToString ("t")));
				timingSection.Add (new StringElement ("latest",
					(DateTime.Today+ distinctTimes.OrderByDescending (o => o.TotalMinutes).First ()).ToString ("t")));
				timingSection.Add (new StringElement ("since most recent",
					checkpoint.SinceLastOccurrence.Humanize(1)+" ago"));



				var detailRoot = new RootElement ("details root");
				var detailsSection = new Section("Occurrence History:");
				detailRoot.Add (detailsSection);
				detailsSection.AddAll (
					checkpoint
					.Occurrences
					.OrderByDescending(o=>o.timeStamp)
					.Select (o => new StringElement (o.timeStamp.ToString ("d"), o.timeStamp.ToString ("t"))));
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
						parent.RespondToModelChanges();
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