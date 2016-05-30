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
		protected CheckPoint LastCheckpointDetailed { get; set;}

		public CheckpointDetailCommand (CheckPointController controller)
		{
			this.Controller = controller;

		}

		public UIViewController GetDetailDialog(CheckPoint Data)
		{
			var checkpoint = Data;

			var root = new RootElement (string.Format ("{0}", checkpoint.Name));
			root.Add(GetDetailSections(Data));

			return new CheckPointDetailViewController (this.Controller,Data, root);
		}

		public Section[] GetDetailSections(CheckPoint checkpoint)
		{
			var distinctTimes = checkpoint.Occurrences.Select (o => o.Time).Distinct();

			var sectionsToReturn = new List<Section> ();

			var timingSection = new Section ("Stats:");
			sectionsToReturn.Add (timingSection);

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

				//var detailRoot = new RootElement ("details root");
				var detailsSection = new Section("Occurrence History:");

				detailsSection.AddAll (
					checkpoint
					.Occurrences
					.OrderByDescending(o=>o.timeStamp)
					.Select (o => new StringElement (o.timeStamp.ToString ("d"), o.timeStamp.ToString ("t"))));

				sectionsToReturn.Add (detailsSection);
			}

			return sectionsToReturn.ToArray ();
		}

		public void ShowDetailDialog(CheckPoint Data)
		{
			var mtd = GetDetailDialog (Data);

			this.LastCheckpointDetailed = Data;

			ShowDetailDialog (mtd, Data);

		}
		public void ShowDetailDialog(UIViewController dialog,CheckPoint Data=null)
		{
			if (Data == null)
				Data = LastCheckpointDetailed;
			
			this.Controller.NavigationController.PushViewController (dialog,true);

			dialog.NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Done,
				(s, e) => this.Controller.NavigationController.PopViewController (true)
			), true);

			CreateOptions (dialog, Data);


		}
		public void CreateOptions(UIViewController dialog,CheckPoint Data)
		{
			var acs = UIAlertController.Create ("options", "stuff to do", UIAlertControllerStyle.ActionSheet);
			acs.AddAction(UIAlertAction.Create("Edit",UIAlertActionStyle.Default,a=>{
				var c = dialog as CheckPointDetailViewController;
				var root = new RootElement("Edit");
				var d = new AddNewCheckpointDialog(this.Controller,root,true);
				d.RenderForCheckPoint(Data);
				this.Controller.NavigationController.PushViewController(d,true);
			}));


			foreach (var cmd in this.Controller.Commands.Commands)
				if (cmd.Value.ShouldDecorate (Data))
					acs.AddAction (cmd.Value.AsAlertAction(c=>
						{if(c.ExecuteFor(this.Controller,Data))
							{	
								//TODO: have to actually update this dialog as well?
								this.Controller.ConditionallyRefreshData();
								CreateOptions(dialog,Data);
							}
						}));

			acs.AddAction (UIAlertAction.Create ("Nevermind", UIAlertActionStyle.Cancel, null));

			dialog.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Edit,
				(s, e) => this.Controller.PresentViewController(acs,true,null)
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

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			this.NavigationItem.HidesBackButton = false;
		}

		public override IUIPreviewActionItem[] PreviewActionItems 
		{
			get {
				return this.actions.ToArray();
			}
		}
	}
}