using System;
using Foundation;
using UIKit;
using ClockKing.Core;
using MonoTouch.Dialog;
using System.Linq;
using System.Collections.Generic;
using Humanizer;

namespace ClockKing
{

	public class CheckPointDetailDialog:DialogViewController
	{

		private List<UIPreviewAction> actions { get; set;}
		private CheckPointController Controller{ get; set;}
		private CheckPoint toDetail { get; set;}

		public CheckPointDetailDialog(UIViewController Parent,CheckPoint toDetail,RootElement root):base(root)
		{
			var parent = Parent as CheckPointController;
			this.Controller = parent;
			this.toDetail = toDetail;
			this.Render ();
		}

		protected void RespondToChanges()
		{
			this.Render ();
			this.Controller.ConditionallyRefreshData (true);
			CreateOptions (this,toDetail);
		}

		protected void Render()
		{
			var sections = GetDetailSections (toDetail, Controller);
			if (!Root.Any ())
				Root.Add (sections);
			else
			{
				var headers = sections.Select (s => s.Header);
				var toRemove = Root.Where (s => headers.Contains (s.Header));
				toRemove.ToList ().ForEach (r => Root.Remove (r,UITableViewRowAnimation.Right));
				//Root.Add (sections);
				Root.Insert (Root.Count (), UITableViewRowAnimation.Left, sections);
			}


			var executor = new Action<Command>((ub)=>
				{
					if(ub.ExecuteFor(this.Controller,toDetail))
						RespondToChanges();
				});

			this.actions = this.Controller.Commands.GetPreviewActionsForCheckpoint (toDetail, executor).ToList();

		}

		public override void ViewDidLoad ()
		{
			ResetNavigation ();
		}

		public void ResetNavigation()
		{
			this.NavigationItem.SetLeftBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Done,
				(s, e) => this.Controller.ResetNavigation()
			), true);

			CreateOptions (this, toDetail);
		}


		public void CreateOptions(UIViewController dialog,CheckPoint Data)
		{
			var acs = UIAlertController.Create (string.Format("options for {0}",Data.Name), "stuff to do", UIAlertControllerStyle.ActionSheet);

			var handler = new Action<Command> ((c) => 
				{
					if(c.ExecuteFor(this.Controller,Data))
						RespondToChanges();
				});

			acs.AddAction (new InPlaceEditCheckPointCommand (this).AsAlertAction (handler));

			this.Controller.Commands.GetAlertActionsForCheckpoint(Data,handler)
				.Where(a=>a.Title!="Edit Goal")
				.ToList()
				.ForEach(cmd=>acs.AddAction (cmd));

			acs.AddAction(UIAlertAction.Create("Nevermind!",UIAlertActionStyle.Cancel,null));

			dialog.NavigationItem.SetRightBarButtonItem (new UIBarButtonItem (UIBarButtonSystemItem.Edit,
				(s, e) => 
				this.Controller.PresentViewController(acs,true,null)
			), true);
		}


		public static Section[] GetDetailSections(CheckPoint checkpoint,CheckPointController Controller)
		{
			var distinctTimes = checkpoint.Occurrences.Select (o => o.Time).Distinct();

			var sectionsToReturn = new List<Section> ();

			var tableCell = new CheckPointCellDialog (checkpoint,Controller);

			var cellHolder = new Section ("Goal:"){ tableCell };
 		
			var Stats = new Section ("Stats:");

			Stats.Add (new StringElement ("Enabled?", checkpoint.Enabled?"Yes":"No"));
			Stats.Add (new StringElement("next",
				"in " + checkpoint.UntilNextTargetTime.Humanize(2)));
			

			sectionsToReturn.Add (cellHolder);
			sectionsToReturn.Add (Stats);


			if (checkpoint.Occurrences.Any ()) 
			{
				Stats.Add (new StringElement ("earliest",
					(DateTime.Today+ distinctTimes.OrderBy (o => o.TotalMinutes).First ()).ToString ("t")));
				Stats.Add (new StringElement ("latest",
					(DateTime.Today+ distinctTimes.OrderByDescending (o => o.TotalMinutes).First ()).ToString ("t")));
				Stats.Add (new StringElement ("since most recent",
					checkpoint.SinceLastOccurrence.Humanize(1)+" ago"));

				var detailsSection = new Section("Occurrence History:");

				detailsSection.AddAll (
					checkpoint
					.Occurrences
					.OrderByDescending(o=>o.timeStamp)
					.Select (o => new StringElement (o.Date.ToString("d"), o.timeStamp.ToString ("t"))));

				sectionsToReturn.Add (detailsSection);
			}

			return sectionsToReturn.ToArray ();
		}


		public override IUIPreviewActionItem[] PreviewActionItems 
		{
			get {
				return this.actions.ToArray();
			}
		}
	}
}

